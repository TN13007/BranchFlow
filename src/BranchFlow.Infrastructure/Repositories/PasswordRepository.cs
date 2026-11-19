using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class PasswordRepository(BranchFlowDbContext db)
{
    public Task<AppUser?> UserAsync(int id, CancellationToken ct) => db.AppUsers.AsNoTracking()
        .Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id && x.IsActive && !x.Deleted && !x.Role.Deleted, ct);

    public Task<AppUser?> UserAsync(string username, CancellationToken ct) => db.AppUsers.AsNoTracking()
        .Include(x => x.Role).SingleOrDefaultAsync(x => x.Username == username && x.IsActive && !x.Deleted && !x.Role.Deleted, ct);

    public Task<bool> ActiveSessionAsync(AppUser user, Guid session, DateTime now, CancellationToken ct) =>
        db.AuthSessions.AsNoTracking().AnyAsync(x => x.Id == session && x.UserId == user.Id &&
            x.CredentialVersion == user.CredentialVersion && x.RevokedAt == null && x.ExpiresAt > now, ct);

    public Task<int> ReplaceAsync(AppUser expected, string hash, bool temporary, DateTime? expires, DateTime now, CancellationToken ct) =>
        db.AppUsers.Where(x => x.Id == expected.Id && x.IsActive && !x.Deleted &&
                x.PasswordHash == expected.PasswordHash && x.CredentialVersion == expected.CredentialVersion)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.PasswordHash, hash)
                .SetProperty(x => x.MustChangePassword, temporary).SetProperty(x => x.TemporaryPasswordExpiresAt, expires)
                .SetProperty(x => x.CredentialVersion, x => x.CredentialVersion + 1).SetProperty(x => x.UpdatedAt, now), ct);

    public async Task RevokeAndResolveAsync(int userId, int actor, DateTime now, CancellationToken ct)
    {
        await db.AuthSessions.Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
        await db.RefreshTokens.Where(x => x.Session.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), ct);
        await db.PasswordResetRequests.Where(x => x.UserId == userId && x.ResolvedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ResolvedAt, now).SetProperty(x => x.ResolvedByUserId, actor), ct);
    }

    // Serializable range lock prevents concurrent anonymous requests from creating duplicates.
    public Task<int> RequestAsync(int userId, DateTime now, CancellationToken ct) => db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT dbo.PasswordResetRequest(UserId, RequestedAt)
        SELECT {userId}, {now} WHERE NOT EXISTS
        (SELECT 1 FROM dbo.PasswordResetRequest WITH (UPDLOCK, HOLDLOCK) WHERE UserId = {userId} AND ResolvedAt IS NULL)
        """, ct);

    public Task<List<PasswordResetRequest>> PendingAsync(CancellationToken ct) => db.PasswordResetRequests.AsNoTracking()
        .Include(x => x.User).ThenInclude(x => x.Role)
        .Where(x => x.ResolvedAt == null && x.User.IsActive && !x.User.Deleted && !x.User.Role.Deleted &&
            (x.User.Role.Code == "BRANCH_ADMIN" || x.User.Role.Code == "EMPLOYEE"))
        .OrderBy(x => x.RequestedAt).Take(100).ToListAsync(ct);

    public void Audit(AuditLog entry) => db.AuditLogs.Add(entry);
}
