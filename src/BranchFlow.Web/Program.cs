using BranchFlow.Web.Components;
using BranchFlow.Web.Services.Api;
using BranchFlow.Web.Services.Auth;
using BranchFlow.Web.Models.Cart;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Blazor Web App với tương tác chạy trên Web server.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<WebSessionStore>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WebSessionStore>());
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<SessionApiTransport>();
builder.Services.AddScoped<OrderApiClient>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<BranchFlow.Web.Services.Preview.WarehousePreviewState>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SessionAuthenticationStateProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "__Host-BranchFlow.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/account";
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = async context =>
    {
        var store = context.HttpContext.RequestServices.GetRequiredService<WebSessionStore>();
        var session = store.Find(context.Principal?.FindFirst(WebSessionStore.SessionClaim)?.Value);
        if (session is null || context.Principal?.Identity?.Name != session.User.Username)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddHttpClient("BranchFlowApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7107/");
    client.Timeout = TimeSpan.FromSeconds(25);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
  .RemoveAllLoggers();

var app = builder.Build();

// Dùng trang lỗi chung và HSTS khi chạy ngoài môi trường phát triển.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapWebAuth();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
