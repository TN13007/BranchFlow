using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Application.Services;

public interface IOrderMenuService
{
    Task<OrderMenu?> GetMenuByBranchAsync(int userId, int branchId, CancellationToken cancellationToken = default);
}
