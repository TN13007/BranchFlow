using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class DemoSeedRepository : IDemoSeedRepository
{
    private readonly BranchFlowDbContext _context;

    public DemoSeedRepository(BranchFlowDbContext context)
    {
        _context = context;
    }

    public Task<Role?> GetRoleByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                role => role.Code == code && !role.Deleted,
                cancellationToken);
    }

    // Ưu tiên bản ghi chưa xóa mềm; nếu chỉ còn bản ghi đã xóa, service sẽ báo xung đột.
    public Task<Branch?> GetBranchByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return _context.Branches
            .AsNoTracking()
            .Where(branch => branch.Code == code)
            .OrderBy(branch => branch.Deleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<AppUser?> GetUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return _context.AppUsers
            .AsNoTracking()
            .Include(user => user.Role)
            .Where(user => user.Username == username)
            .OrderBy(user => user.Deleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Không dùng AsNoTracking vì service cần cập nhật PasswordHash của tài khoản demo.
    public Task<List<AppUser>> GetUsersByUsernamesAsync(
        IReadOnlyCollection<string> usernames,
        CancellationToken cancellationToken = default)
    {
        return _context.AppUsers
            .Include(user => user.Role)
            .Where(user => usernames.Contains(user.Username))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeProfile?> GetEmployeeProfileByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _context.EmployeeProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .OrderBy(profile => profile.Deleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // A new seed assignment has no end date, so any existing assignment ending after
    // the proposed start time would overlap it, including assignments scheduled for the future.
    public Task<AdminBranchAssignment?> GetAssignmentOverlappingFromUtcByBranchIdAsync(
        int branchId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken = default)
    {
        return _context.AdminBranchAssignments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.BranchId == branchId &&
                    (assignment.EffectiveTo == null || assignment.EffectiveTo > effectiveFromUtc),
                cancellationToken);
    }

    public Task<EmployeeBranchAssignment?> GetAssignmentOverlappingFromUtcByProfileIdAsync(
        int profileId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken = default)
    {
        return _context.EmployeeBranchAssignments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.EmployeeProfileId == profileId &&
                    (assignment.EffectiveTo == null || assignment.EffectiveTo > effectiveFromUtc),
                cancellationToken);
    }

    public void AddBranch(Branch branch) => _context.Branches.Add(branch);

    public void AddUser(AppUser user) => _context.AppUsers.Add(user);

    public void AddEmployeeProfile(EmployeeProfile profile) => _context.EmployeeProfiles.Add(profile);

    public void AddAdminBranchAssignment(AdminBranchAssignment assignment) =>
        _context.AdminBranchAssignments.Add(assignment);

    public void AddEmployeeBranchAssignment(EmployeeBranchAssignment assignment) =>
        _context.EmployeeBranchAssignments.Add(assignment);
}
