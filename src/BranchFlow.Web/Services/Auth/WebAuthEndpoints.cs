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
        app.MapPost("/auth/forgot-password", ForgotAsync).AllowAnonymous();
        app.MapPost("/auth/change-password", ChangePasswordAsync).RequireAuthorization();
    }

    private static async Task<IResult> LoginAsync(HttpContext context, IAntiforgery antiforgery, AuthApiClient api, WebSessionStore store)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.IsHttps) return Results.BadRequest("Đăng nhập yêu cầu HTTPS.");
        if (!await ValidateAntiforgeryAsync(context, antiforgery))
            return Results.BadRequest("Form đã hết hạn. Vui lòng tải lại trang.");
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
            return Results.LocalRedirect(user.MustChangePassword ? "/change-password" : BranchFlow.Web.Models.Appearance.WorkspaceNavigation.Home(user.Role));
        }
        catch (ApiException ex)
        {
            if (newKey is not null) store.Remove(newKey);
            var error = ex.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "credentials",
                HttpStatusCode.TooManyRequests => "rate",
                HttpStatusCode.ServiceUnavailable => "unavailable",
                _ => "service"
            };
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
        if (!await ValidateAntiforgeryAsync(context, antiforgery))
            return Results.BadRequest("Form đã hết hạn. Vui lòng tải lại trang.");
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

    private static async Task<IResult> ForgotAsync(HttpContext context, IAntiforgery antiforgery, AuthApiClient api)
    {
        if (!context.Request.IsHttps || !await ValidateAntiforgeryAsync(context, antiforgery))
            return Results.BadRequest("Form không hợp lệ. Vui lòng tải lại trang bằng HTTPS.");
        var fields = await context.Request.ReadFormAsync(context.RequestAborted);
        try
        {
            await api.ForgotAsync(fields["username"].ToString(), context.RequestAborted);
            return Results.LocalRedirect("/forgot-password?sent=true");
        }
        catch (ApiException ex) { return Results.LocalRedirect(ex.StatusCode == HttpStatusCode.TooManyRequests ? "/forgot-password?error=rate" : "/forgot-password?error=service"); }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException) { return Results.LocalRedirect("/forgot-password?error=service"); }
    }

    private static async Task<IResult> ChangePasswordAsync(HttpContext context, IAntiforgery antiforgery, AuthApiClient api, WebSessionStore store)
    {
        if (!context.Request.IsHttps || !await ValidateAntiforgeryAsync(context, antiforgery))
            return Results.BadRequest("Form không hợp lệ. Vui lòng tải lại trang bằng HTTPS.");
        var session = store.Find(context.User.FindFirst(WebSessionStore.SessionClaim)?.Value);
        if (session is null) return Results.LocalRedirect("/login?notice=session");
        var fields = await context.Request.ReadFormAsync(context.RequestAborted);
        await session.Gate.WaitAsync(context.RequestAborted);
        try
        {
            if (session.Revoked) return Results.LocalRedirect("/login?notice=session");
            var tokens = session.Tokens;
            if (tokens.AccessTokenExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(30))
                session.Tokens = tokens = await api.RefreshAsync(tokens.RefreshToken, context.RequestAborted);
            await api.ChangePasswordAsync(tokens.AccessToken, new ChangePasswordRequest(fields["currentPassword"].ToString(),
                fields["newPassword"].ToString(), fields["confirmPassword"].ToString()), context.RequestAborted);
            store.RemoveForUser(session.User.UserId);
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("/login?notice=password-changed");
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Conflict or HttpStatusCode.Forbidden)
            {
                store.RemoveForUser(session.User.UserId);
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.LocalRedirect("/login?notice=session");
            }
            return Results.LocalRedirect(ex.StatusCode == HttpStatusCode.TooManyRequests ? "/change-password?error=rate" : "/change-password?error=invalid");
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException)
        { return Results.LocalRedirect("/change-password?error=service"); }
        finally { session.Gate.Release(); }
    }

    public static async Task<bool> ValidateAntiforgeryAsync(HttpContext context, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException) when (
            context.Items.TryGetValue(WebSessionStore.RejectedPrincipalKey, out var item) &&
            item is ClaimsPrincipal rejected)
        {
            var current = context.User;
            try
            {
                context.User = rejected;
                await antiforgery.ValidateRequestAsync(context);
                return true;
            }
            catch (AntiforgeryValidationException)
            {
                return false;
            }
            finally
            {
                context.User = current;
            }
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
