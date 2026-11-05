using System.Text;
using BranchFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BranchFlow.Api.Security;

public static class JwtConfiguration
{
    // Chỉ nhận JWT đúng chữ ký/issuer/audience và phiên DB còn hiệu lực.
    public static void AddBranchFlowJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32 ||
            string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Cấu hình Jwt:Key (ít nhất 32 byte), Jwt:Issuer và Jwt:Audience bằng user-secrets hoặc biến môi trường. Xem docs/Menu-API-handoff.md.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.IncludeErrorDetails = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = issuer,
                ValidateAudience = true, ValidAudience = audience,
                ValidateLifetime = true, RequireExpirationTime = true,
                ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.Zero, NameClaimType = "sub", RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    System.Security.Claims.ClaimsPrincipal? user = context.Principal;
                    string? role = user?.FindFirst("role")?.Value;

                    if (!int.TryParse(user?.FindFirst("sub")?.Value, out int userId) ||
                        userId <= 0 ||
                        !Guid.TryParse(user?.FindFirst("sid")?.Value, out Guid sessionId) ||
                        !IsSupportedRole(role))
                    {
                        context.Fail("Token không hợp lệ.");
                        return;
                    }

                    bool activeSession = await context.HttpContext.RequestServices
                        .GetRequiredService<IMenuAccessRepository>()
                        .IsActiveSessionAsync(
                            userId,
                            sessionId,
                            role!,
                            context.HttpContext.RequestAborted);

                    if (!activeSession)
                    {
                        context.Fail("Phiên không hợp lệ.");
                    }
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await Results.Problem(statusCode: 401, title: "Chưa xác thực",
                        detail: "Vui lòng đăng nhập lại.", extensions: new Dictionary<string, object?>
                        { ["traceId"] = context.HttpContext.TraceIdentifier }).ExecuteAsync(context.HttpContext);
                },
                OnForbidden = context => Results.Problem(statusCode: 403, title: "Không có quyền",
                    detail: "Bạn không có quyền thực hiện thao tác này.", extensions: new Dictionary<string, object?>
                    { ["traceId"] = context.HttpContext.TraceIdentifier }).ExecuteAsync(context.HttpContext)
            };
        });
    }

    private static bool IsSupportedRole(string? role)
    {
        return role is "OWNER" or "SYSTEM_ADMIN" or "BRANCH_ADMIN" or "EMPLOYEE";
    }
}
