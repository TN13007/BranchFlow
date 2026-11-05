using BranchFlow.Infrastructure.Models;

namespace BranchFlow.Infrastructure.Repositories;

public interface IDemoSeedRepository
{
    Task<Role?> GetRoleByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Branch?> GetBranchByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<AppUser?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<AppUser>> GetUsersByUsernamesAsync(IReadOnlyCollection<string> usernames, CancellationToken cancellationToken = default);
    Task<EmployeeProfile?> GetEmployeeProfileByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<AdminBranchAssignment?> GetAssignmentOverlappingFromUtcByBranchIdAsync(int branchId, DateTime effectiveFromUtc, CancellationToken cancellationToken = default);
    Task<EmployeeBranchAssignment?> GetAssignmentOverlappingFromUtcByProfileIdAsync(int profileId, DateTime effectiveFromUtc, CancellationToken cancellationToken = default);

    void AddBranch(Branch branch);
    void AddUser(AppUser user);
    void AddEmployeeProfile(EmployeeProfile profile);
    void AddAdminBranchAssignment(AdminBranchAssignment assignment);
    void AddEmployeeBranchAssignment(EmployeeBranchAssignment assignment);
}
