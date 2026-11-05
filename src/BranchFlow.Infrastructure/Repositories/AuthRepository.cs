using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly BranchFlowDbContext _context;
    private readonly TimeProvider _clock;

    public AuthRepository(BranchFlowDbContext context, TimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    // Chỉ trả về tài khoản đang hoạt động để luồng đăng nhập không dùng tài khoản bị khóa.
    public Task<AuthUserData?> GetActiveUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult<AuthUserData?>(null);
        }

        string normalizedUsername = username.Trim();

        return _context.AppUsers
            .AsNoTracking()
            .Where(user =>
                user.Username == normalizedUsername &&
                user.IsActive &&
                !user.Deleted &&
                !user.Role.Deleted)
            .Select(user => new AuthUserData(
                user.Id,
                user.Username,
                user.PasswordHash,
                user.FullName,
                user.Role.Code))
            .FirstOrDefaultAsync(cancellationToken);
    }


    // Read the complete refresh-token/session/user state without tracking it.
    public Task<RefreshToken?> GetRefreshTokenByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .AsNoTracking()
            .Include(refreshToken => refreshToken.Session)
                .ThenInclude(session => session.User)
                    .ThenInclude(user => user.Role)
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    // UsedAt and RowVersion form the atomic one-time-use guard for refresh rotation.
    public async Task<bool> TryMarkRefreshTokenUsedAsync(
        Guid refreshTokenId,
        byte[] expectedRowVersion,
        DateTime usedAtUtc,
        CancellationToken cancellationToken = default)
    {
        int affectedRows = await _context.RefreshTokens
            .Where(refreshToken =>
                refreshToken.Id == refreshTokenId &&
                refreshToken.UsedAt == null &&
                refreshToken.RevokedAt == null &&
                refreshToken.RowVersion == expectedRowVersion)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(refreshToken => refreshToken.UsedAt, usedAtUtc),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task RevokeSessionAsync(
        Guid sessionId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await _context.AuthSessions
            .Where(session => session.Id == sessionId && session.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(session => session.RevokedAt, revokedAtUtc),
                cancellationToken);
    }

    public void AddSession(AuthSession session)
    {
        _context.AuthSessions.Add(session);
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
    }

    // Trả về các chi nhánh mà tài khoản đang có quyền truy cập tại thời điểm UTC hiện tại.
    public async Task<IReadOnlyList<AccessibleBranchData>> GetAccessibleBranchesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Array.Empty<AccessibleBranchData>();
        }

        string? roleCode = await _context.AppUsers
            .AsNoTracking()
            .Where(user =>
                user.Id == userId &&
                user.IsActive &&
                !user.Deleted &&
                !user.Role.Deleted)
            .Select(user => user.Role.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (roleCode is null)
        {
            return Array.Empty<AccessibleBranchData>();
        }

        DateTime now = _clock.GetUtcNow().UtcDateTime;
        IQueryable<Branch> accessibleBranches;

        switch (roleCode)
        {
            case "OWNER":
            case "SYSTEM_ADMIN":
                accessibleBranches = _context.Branches.AsNoTracking();
                break;

            case "BRANCH_ADMIN":
                accessibleBranches = _context.AdminBranchAssignments
                    .AsNoTracking()
                    .Where(assignment =>
                        assignment.AdminUserId == userId &&
                        assignment.EffectiveFrom <= now &&
                        (assignment.EffectiveTo == null || assignment.EffectiveTo > now))
                    .Select(assignment => assignment.Branch);
                break;

            case "EMPLOYEE":
                accessibleBranches = _context.EmployeeBranchAssignments
                    .AsNoTracking()
                    .Where(assignment =>
                        assignment.EmployeeProfile.UserId == userId &&
                        !assignment.EmployeeProfile.Deleted &&
                        assignment.EffectiveFrom <= now &&
                        (assignment.EffectiveTo == null || assignment.EffectiveTo > now))
                    .Select(assignment => assignment.Branch);
                break;

            default:
                return Array.Empty<AccessibleBranchData>();
        }

        return await accessibleBranches
            .Where(branch => branch.IsActive && !branch.Deleted)
            .OrderBy(branch => branch.Code)
            .Select(branch => new AccessibleBranchData(
                branch.Id,
                branch.Code,
                branch.Name))
            .ToListAsync(cancellationToken);
    }
}
