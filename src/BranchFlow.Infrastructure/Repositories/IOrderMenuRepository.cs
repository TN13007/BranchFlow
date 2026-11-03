namespace BranchFlow.Infrastructure.Repositories;

public interface IOrderMenuRepository
{
    Task<OrderMenuData?> GetMenuByBranchAsync(int branchId,CancellationToken cancellationToken = default);
}