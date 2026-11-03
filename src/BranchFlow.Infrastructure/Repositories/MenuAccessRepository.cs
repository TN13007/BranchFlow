using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class MenuAccessRepository(BranchFlowDbContext context, TimeProvider clock) : IMenuAccessRepository
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

    // Phân công có hiệu lực theo khoảng UTC [EffectiveFrom, EffectiveTo).
    public Task<bool> CanReadMenuAsync(int userId, int branchId, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return context.EmployeeBranchAssignments.AsNoTracking().AnyAsync(a =>
            a.BranchId == branchId && a.EmployeeProfile.UserId == userId &&
            !a.EmployeeProfile.Deleted && a.EmployeeProfile.User.IsActive &&
            !a.EmployeeProfile.User.Deleted && !a.EmployeeProfile.User.Role.Deleted &&
            a.EmployeeProfile.User.Role.Code == "EMPLOYEE" && a.EffectiveFrom <= now &&
            (a.EffectiveTo == null || a.EffectiveTo > now), cancellationToken);
    }
}
