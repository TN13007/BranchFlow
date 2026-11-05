using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using BranchFlow.Api.Security;
using BranchFlow.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Data.SqlClient;
using System.Globalization;

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
builder.Services.AddScoped<ISalesDemoSeedRepository, SalesDemoSeedRepository>();
builder.Services.AddScoped<ISalesDemoSeedService, SalesDemoSeedService>();
builder.Services.AddScoped<IOrderMenuRepository, OrderMenuRepository>();
builder.Services.AddScoped<IOrderMenuService, OrderMenuService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
// QA clock/TTL overrides are accepted only by a Testing host bound to local BranchFlow_Test.
string? qaUtc = builder.Configuration["Qa:UtcNow"];
int? qaAccessSeconds = builder.Configuration.GetValue<int?>("Qa:AccessTokenLifetimeSeconds");
int? qaSessionSeconds = builder.Configuration.GetValue<int?>("Qa:SessionLifetimeSeconds");
bool hasQaOverride = qaUtc is not null || qaAccessSeconds.HasValue || qaSessionSeconds.HasValue;
if (hasQaOverride)
{
    var sql = new SqlConnectionStringBuilder(connectionString);
    if (!builder.Environment.IsEnvironment("Testing") ||
        !sql.InitialCatalog.Equals("BranchFlow_Test", StringComparison.OrdinalIgnoreCase) ||
        !(sql.DataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
          sql.DataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)))
        throw new InvalidOperationException("QA time overrides require Testing and local BranchFlow_Test.");
    if ((qaAccessSeconds.HasValue || qaSessionSeconds.HasValue) &&
        (qaAccessSeconds is < 1 or > 3600 || qaSessionSeconds is < 2 or > 86400 ||
         qaAccessSeconds is null || qaSessionSeconds is null || qaSessionSeconds <= qaAccessSeconds))
        throw new InvalidOperationException("QA token lifetimes are invalid.");
}
TimeProvider apiClock = qaUtc is null ? TimeProvider.System :
    new FixedUtcTimeProvider(DateTimeOffset.Parse(qaUtc, CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
builder.Services.AddSingleton(apiClock);
builder.Services.AddScoped<IMenuAccessRepository, MenuAccessRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton(new AuthTokenSettings(
    builder.Configuration["Jwt:Key"] ?? string.Empty,
    builder.Configuration["Jwt:Issuer"] ?? string.Empty,
    builder.Configuration["Jwt:Audience"] ?? string.Empty,
    qaAccessSeconds.HasValue ? TimeSpan.FromSeconds(qaAccessSeconds.Value) : null,
    qaSessionSeconds.HasValue ? TimeSpan.FromSeconds(qaSessionSeconds.Value) : null));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBranchAccessService, BranchAccessService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddBranchFlowJwt(builder.Configuration);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        return ValueTask.CompletedTask;
    };
});


// Đăng ký controller và bộ sinh tài liệu Swagger cho API.
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BranchFlow API",
        Version = "v1",
        Description = "BranchFlow API: xác thực JWT và nghiệp vụ quản lý chuỗi cửa hàng."
    });

    string xmlFileName = $"{builder.Environment.ApplicationName}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

var app = builder.Build();
// Lỗi ngoài dự kiến luôn trả ProblemDetails an toàn, không trả stack/SQL cho client.
app.UseExceptionHandler();

// Không lưu cache response của login/refresh, kể cả lỗi model binding hoặc rate limit.
app.Use(async (context, next) =>
{
    string? path = context.Request.Path.Value?.TrimEnd('/');
    if (string.Equals(path, "/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(path, "/api/auth/refresh", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers.CacheControl = "no-store";
    }

    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

// Chỉ mở tài liệu Swagger trong môi trường phát triển.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BranchFlow API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
