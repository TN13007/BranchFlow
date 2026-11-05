using BranchFlow.Infrastructure.Models;

namespace BranchFlow.Infrastructure.Repositories;

public interface IAuthRepository
{
    Task<AuthUserData?> GetActiveUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<AuthCurrentUserData?> GetActiveUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccessibleBranchData>> GetAccessibleBranchesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> TryMarkRefreshTokenUsedAsync(
        Guid refreshTokenId,
        byte[] expectedRowVersion,
        DateTime usedAtUtc,
        CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(
        Guid sessionId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeCurrentSessionAsync(
        Guid sessionId,
        int userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default);

    void AddSession(AuthSession session);
    void AddRefreshToken(RefreshToken refreshToken);
}
