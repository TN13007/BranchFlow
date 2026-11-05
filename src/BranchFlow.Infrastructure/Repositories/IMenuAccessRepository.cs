namespace BranchFlow.Infrastructure.Repositories;

public interface IMenuAccessRepository
{
    Task<bool> IsActiveSessionAsync(int userId, Guid sessionId, string role, CancellationToken cancellationToken = default);
}
