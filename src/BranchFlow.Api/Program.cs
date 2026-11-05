using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using BranchFlow.Api.Security;
using BranchFlow.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Dùng DB riêng của nhóm; dừng rõ ràng nếu máy chưa cấu hình connection string.
string? connectionString = builder.Configuration.GetConnectionString("BranchFlowDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Chưa cấu hình ConnectionStrings:BranchFlowDb. Xem database/README.md.");
}
builder.Services.AddDbContext<BranchFlowDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký các thành phần dùng chung DbContext theo cùng request scope.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderMenuRepository, OrderMenuRepository>();
builder.Services.AddScoped<IOrderMenuService, OrderMenuService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IMenuAccessRepository, MenuAccessRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddBranchFlowJwt(builder.Configuration);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

// Đăng ký controller và bộ sinh tài liệu Swagger cho API.
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BranchFlow API",
        Version = "v1",
        Description = "API dự án nhóm BranchFlow. Hiện mới thiết lập cấu trúc nền."
    });

    string xmlFileName = $"{builder.Environment.ApplicationName}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

var app = builder.Build();
// Lỗi ngoài dự kiến luôn trả ProblemDetails an toàn, không trả stack/SQL cho client.
app.UseExceptionHandler();

// Chỉ mở tài liệu Swagger trong môi trường phát triển.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BranchFlow API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
