using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Repositories;

namespace BranchFlow.Application.Services;

public sealed class BranchAccessService : IBranchAccessService
{
    private readonly IAuthRepository _authRepository;

    public BranchAccessService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<IReadOnlyList<AccessibleBranch>> GetAccessibleBranchesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Array.Empty<AccessibleBranch>();
        }

        IReadOnlyList<AccessibleBranchData> branchData =
            await _authRepository.GetAccessibleBranchesAsync(userId, cancellationToken);

        List<AccessibleBranch> branches = new();

        foreach (AccessibleBranchData branch in branchData)
        {
            branches.Add(new AccessibleBranch(branch.Id, branch.Code, branch.Name));
        }

        return branches;
    }

    public async Task<bool> CanAccessBranchAsync(
        int userId,
        int branchId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || branchId <= 0)
        {
            return false;
        }

        IReadOnlyList<AccessibleBranch> branches =
            await GetAccessibleBranchesAsync(userId, cancellationToken);

        foreach (AccessibleBranch branch in branches)
        {
            if (branch.Id == branchId)
            {
                return true;
            }
        }

        return false;
    }
}
