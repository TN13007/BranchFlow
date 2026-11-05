using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class MenuAccessRepository(BranchFlowDbContext context) : IMenuAccessRepository
{
    // Kiểm tra lại phiên, tài khoản và role trong DB ở mỗi request đã xác thực JWT.
    public Task<bool> IsActiveSessionAsync(int userId, Guid sessionId, string role, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return context.AuthSessions.AsNoTracking().AnyAsync(s =>
            s.Id == sessionId && s.UserId == userId && s.CreatedAt <= now &&
            s.RevokedAt == null && s.ExpiresAt > now &&
            s.User.IsActive && !s.User.Deleted && !s.User.Role.Deleted &&
            s.User.Role.Code == role, cancellationToken);
    }

}
