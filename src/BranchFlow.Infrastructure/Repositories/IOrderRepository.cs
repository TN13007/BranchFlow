using BranchFlow.Infrastructure.Models;

namespace BranchFlow.Infrastructure.Repositories;

public interface IOrderRepository
{
    Task<(string Code, int Sequence)> AllocateCodeAsync(int branchId, DateOnly date, CancellationToken ct);
    void Add(SalesOrder order);
    Task<List<SalesOrder>> GetMineAsync(int userId, IReadOnlyList<int> branchIds, string? keyword, DateOnly? date, CancellationToken ct);
    Task<SalesOrder?> GetDetailAsync(int id, int userId, CancellationToken ct);
}
