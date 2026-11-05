namespace BranchFlow.Infrastructure.Repositories;

public interface IAuthRepository
{
    Task<AuthUserData?> GetActiveUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccessibleBranchData>> GetAccessibleBranchesAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
