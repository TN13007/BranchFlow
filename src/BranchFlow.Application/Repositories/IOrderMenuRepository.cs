using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Application.Repositories;

public interface IOrderMenuRepository
{
    Task<OrderMenu?> GetMenuByBranchAsync(int branchId,CancellationToken cancellationToken = default);
}