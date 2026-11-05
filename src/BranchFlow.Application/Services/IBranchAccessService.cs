using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Application.Services;

public interface IBranchAccessService
{
    Task<IReadOnlyList<AccessibleBranch>> GetAccessibleBranchesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> CanAccessBranchAsync(
        int userId,
        int branchId,
        CancellationToken cancellationToken = default);

    Task<bool> HasBranchScopeAsync(
        int userId,
        int branchId,
        CancellationToken cancellationToken = default);
}
