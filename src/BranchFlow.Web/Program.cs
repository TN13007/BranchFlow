using BranchFlow.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Blazor Web App với tương tác chạy trên Web server.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Dùng trang lỗi chung và HSTS khi chạy ngoài môi trường phát triển.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
