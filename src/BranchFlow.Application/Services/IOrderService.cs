using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Application.Services;

public interface IOrderService
{
    Task<CreatedOrder> CreateAsync(int userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<List<OrderSummary>> GetMineAsync(int userId, string? keyword, DateOnly? date, CancellationToken ct = default);
    Task<OrderDetail?> GetDetailAsync(int userId, int id, CancellationToken ct = default);
}

public sealed class OrderValidationException(string message) : Exception(message);
public sealed class OrderBranchUnavailableException : Exception;
