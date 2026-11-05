using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BranchFlow.Application.Services;

public sealed record AuthTokenSettings(string Key, string Issuer, string Audience);

public sealed class AuthService : IAuthService
{
    private const int AccessTokenLifetimeMinutes = 15;
    private const int SessionLifetimeDays = 7;
    private const int RefreshTokenRandomBytes = 64;

    private readonly IAuthRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly TimeProvider _clock;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _signingKey;

    public AuthService(
        IAuthRepository repository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher,
        AuthTokenSettings tokenSettings,
        TimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _issuer = tokenSettings.Issuer;
        _audience = tokenSettings.Audience;

        if (string.IsNullOrWhiteSpace(tokenSettings.Key) ||
            Encoding.UTF8.GetByteCount(tokenSettings.Key) < 32 ||
            string.IsNullOrWhiteSpace(_issuer) ||
            string.IsNullOrWhiteSpace(_audience))
        {
            throw new InvalidOperationException(
                "Cấu hình Jwt:Key (ít nhất 32 byte), Jwt:Issuer và Jwt:Audience trước khi phát hành token.");
        }

        _signingKey = Encoding.UTF8.GetBytes(tokenSettings.Key);
    }

    // Kiểm tra thông tin đăng nhập trước, sau đó tạo session và refresh token trong một transaction.
    public async Task<TokenPair?> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        AuthUserData? user = await _repository.GetActiveUserByUsernameAsync(
            username.Trim(),
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        AppUser passwordUser = new()
        {
            Id = user.Id,
            Username = user.Username
        };

        PasswordVerificationResult verification = _passwordHasher.VerifyHashedPassword(
            passwordUser,
            user.PasswordHash,
            password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        DateTimeOffset now = _clock.GetUtcNow();
        DateTime nowUtc = now.UtcDateTime;
        Guid sessionId = Guid.NewGuid();
        DateTimeOffset sessionExpiresAt = now.AddDays(SessionLifetimeDays);
        string rawRefreshToken = GenerateRefreshToken();

        AuthSession session = new()
        {
            Id = sessionId,
            UserId = user.Id,
            CreatedAt = nowUtc,
            ExpiresAt = sessionExpiresAt.UtcDateTime
        };

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            TokenHash = HashRefreshToken(rawRefreshToken),
            CreatedAt = nowUtc,
            ExpiresAt = sessionExpiresAt.UtcDateTime
        };

        DateTimeOffset accessTokenExpiresAt = GetAccessTokenExpiry(now, sessionExpiresAt);
        TokenPair tokenPair = CreateTokenPair(
            user.Id,
            user.RoleCode,
            sessionId,
            now,
            accessTokenExpiresAt,
            rawRefreshToken,
            sessionExpiresAt);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            _repository.AddSession(session);
            _repository.AddRefreshToken(refreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return tokenPair;
        }
        catch
        {
            await RollbackSafelyAsync();
            throw;
        }
    }

    // Đánh dấu token cũ đã dùng trước khi phát hành token kế tiếp; RowVersion ngăn hai request cùng thắng.
    public async Task<TokenPair?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        DateTimeOffset now = _clock.GetUtcNow();
        DateTime nowUtc = now.UtcDateTime;
        string tokenHash = HashRefreshToken(refreshToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            RefreshToken? currentToken = await _repository.GetRefreshTokenByHashAsync(
                tokenHash,
                cancellationToken);

            if (currentToken is null)
            {
                await RollbackSafelyAsync();
                return null;
            }

            if (currentToken.UsedAt is not null)
            {
                await _repository.RevokeSessionAsync(
                    currentToken.SessionId,
                    nowUtc,
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return null;
            }

            AuthSession session = currentToken.Session;
            AppUser user = session.User;

            if (session.RevokedAt is not null ||
                session.ExpiresAt <= nowUtc ||
                currentToken.RevokedAt is not null ||
                currentToken.ExpiresAt <= nowUtc)
            {
                await RollbackSafelyAsync();
                return null;
            }

            if (!user.IsActive || user.Deleted || user.Role.Deleted)
            {
                await _repository.RevokeSessionAsync(
                    session.Id,
                    nowUtc,
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return null;
            }

            bool markedAsUsed = await _repository.TryMarkRefreshTokenUsedAsync(
                currentToken.Id,
                currentToken.RowVersion,
                nowUtc,
                cancellationToken);

            if (!markedAsUsed)
            {
                RefreshToken? latestToken = await _repository.GetRefreshTokenByHashAsync(
                    tokenHash,
                    cancellationToken);

                if (latestToken?.UsedAt is not null)
                {
                    await _repository.RevokeSessionAsync(
                        latestToken.SessionId,
                        nowUtc,
                        cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                else
                {
                    await RollbackSafelyAsync();
                }

                return null;
            }

            DateTimeOffset absoluteSessionExpiry = ToUtcOffset(session.ExpiresAt);
            if (absoluteSessionExpiry <= now)
            {
                await RollbackSafelyAsync();
                return null;
            }

            string nextRawRefreshToken = GenerateRefreshToken();
            RefreshToken nextRefreshToken = new()
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                TokenHash = HashRefreshToken(nextRawRefreshToken),
                CreatedAt = nowUtc,
                ExpiresAt = absoluteSessionExpiry.UtcDateTime
            };

            DateTimeOffset accessTokenExpiresAt = GetAccessTokenExpiry(now, absoluteSessionExpiry);
            TokenPair tokenPair = CreateTokenPair(
                user.Id,
                user.Role.Code,
                session.Id,
                now,
                accessTokenExpiresAt,
                nextRawRefreshToken,
                absoluteSessionExpiry);

            _repository.AddRefreshToken(nextRefreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return tokenPair;
        }
        catch
        {
            await RollbackSafelyAsync();
            throw;
        }
    }

    private TokenPair CreateTokenPair(
        int userId,
        string roleCode,
        Guid sessionId,
        DateTimeOffset issuedAt,
        DateTimeOffset accessTokenExpiresAt,
        string rawRefreshToken,
        DateTimeOffset refreshTokenExpiresAt)
    {
        return new TokenPair
        {
            AccessToken = CreateAccessToken(
                userId,
                roleCode,
                sessionId,
                issuedAt,
                accessTokenExpiresAt),
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private string CreateAccessToken(
        int userId,
        string roleCode,
        Guid sessionId,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        Claim[] claims =
        [
            new Claim("sub", userId.ToString(CultureInfo.InvariantCulture)),
            new Claim("sid", sessionId.ToString("D")),
            new Claim("role", roleCode)
        ];

        SigningCredentials signingCredentials = new(
            new SymmetricSecurityKey(_signingKey),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwt = new(
            _issuer,
            _audience,
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static DateTimeOffset GetAccessTokenExpiry(
        DateTimeOffset now,
        DateTimeOffset sessionExpiresAt)
    {
        DateTimeOffset normalExpiry = now.AddMinutes(AccessTokenLifetimeMinutes);
        return normalExpiry < sessionExpiresAt ? normalExpiry : sessionExpiresAt;
    }

    private static string GenerateRefreshToken()
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenRandomBytes));
    }

    private static string HashRefreshToken(string rawRefreshToken)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken));
        return Convert.ToHexString(hash);
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private async Task RollbackSafelyAsync()
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
        }
        catch
        {
            // Giữ nguyên lỗi gốc; transaction đã commit/đã được giải phóng thì không rollback lần nữa.
        }
    }
}
