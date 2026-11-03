using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Web.Models.Auth;
using BranchFlow.Web.Services.Api;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BranchFlow.Web.Services.Auth;

public static class WebAuthEndpoints
{
    public static void MapWebAuth(this WebApplication app)
    {
        app.MapPost("/auth/login", LoginAsync).AllowAnonymous();
        app.MapPost("/auth/logout", LogoutAsync);
    }

    private static async Task<IResult> LoginAsync(HttpContext context, IAntiforgery antiforgery, AuthApiClient api, WebSessionStore store)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.IsHttps) return Results.BadRequest("Đăng nhập yêu cầu HTTPS.");
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest("Form đã hết hạn. Vui lòng tải lại trang."); }
        var fields = await context.Request.ReadFormAsync(context.RequestAborted);
        var model = new LoginFormModel { Username = fields["username"].ToString().Trim(), Password = fields["password"].ToString() };
        if (!Validator.TryValidateObject(model, new ValidationContext(model), [], true))
            return Results.LocalRedirect("/login?error=invalid");
        string? newKey = null;
        try
        {
            var pair = await api.LoginAsync(new LoginRequest(model.Username, model.Password), context.RequestAborted);
            if (!store.ValidTokens(pair)) return Results.LocalRedirect("/login?error=service");
            var user = await api.MeAsync(pair.AccessToken, context.RequestAborted);
            newKey = store.Add(user, pair);
            var session = store.Find(newKey)!;
            // Cookie chỉ chứa định danh và khóa store; token không đi xuống browser.
            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(WebSessionStore.SessionClaim, newKey)
            ], CookieAuthenticationDefaults.AuthenticationScheme);
            var oldKey = context.User.FindFirst(WebSessionStore.SessionClaim)?.Value;
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = session.ExpiresAt, AllowRefresh = false });
            if (oldKey is not null) store.Remove(oldKey);
            return Results.LocalRedirect(user.Role == "EMPLOYEE" ? "/orders/create" : "/account");
        }
        catch (ApiException ex)
        {
            if (newKey is not null) store.Remove(newKey);
            var error = ex.StatusCode == HttpStatusCode.Unauthorized ? "credentials" : ex.StatusCode == HttpStatusCode.TooManyRequests ? "rate" : "service";
            return Results.LocalRedirect($"/login?error={error}");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or InvalidOperationException)
        {
            if (newKey is not null) store.Remove(newKey);
            return Results.LocalRedirect("/login?error=service");
        }
        finally { model.Password = string.Empty; }
    }

    private static async Task<IResult> LogoutAsync(HttpContext context, IAntiforgery antiforgery, WebSessionStore store, AuthApiClient api)
    {
        context.Response.Headers.CacheControl = "no-store";
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest("Form đã hết hạn. Vui lòng tải lại trang."); }
        var key = context.User.FindFirst(WebSessionStore.SessionClaim)?.Value;
        var session = store.Find(key);
        var remoteFailed = false;
        if (session is not null && key is not null)
        {
            store.Remove(key);
            await session.Gate.WaitAsync(context.RequestAborted);
            try
            {
                var token = session.Tokens;
                if (token.AccessTokenExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(30))
                    token = await api.RefreshAsync(token.RefreshToken, context.RequestAborted);
                await api.LogoutAsync(token.AccessToken, context.RequestAborted);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized) { }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or OperationCanceledException or JsonException) { remoteFailed = true; }
            finally { session.Gate.Release(); }
        }
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.LocalRedirect(remoteFailed ? "/login?notice=local-logout" : "/login?notice=logout");
    }
}
