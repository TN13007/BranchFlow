using System.Data;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class OrderRepository(BranchFlowDbContext db) : IOrderRepository
{
    // Procedure dùng transaction hiện hành; counter và toàn bộ đơn cùng commit/rollback.
    public async Task<(string Code, int Sequence)> AllocateCodeAsync(int branchId, DateOnly date, CancellationToken ct)
    {
        var branch = new SqlParameter("@BranchId", branchId);
        var businessDate = new SqlParameter("@BusinessDate", SqlDbType.Date) { Value = date.ToDateTime(TimeOnly.MinValue) };
        var code = new SqlParameter("@OrderCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
        var sequence = new SqlParameter("@SequenceNumber", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await db.Database.ExecuteSqlRawAsync("EXEC dbo.usp_GetNextOrderCode @BranchId, @BusinessDate, @OrderCode OUTPUT, @SequenceNumber OUTPUT", [branch, businessDate, code, sequence], ct);
        return ((string)code.Value, (int)sequence.Value);
    }

    public void Add(SalesOrder order) => db.SalesOrders.Add(order);

    // Chỉ trả đơn của chính nhân viên, trong các chi nhánh hiện còn được truy cập.
    public Task<List<SalesOrder>> GetMineAsync(int userId, IReadOnlyList<int> branchIds, string? keyword, DateOnly? date, CancellationToken ct)
    {
        var query = db.SalesOrders.AsNoTracking().Where(o => o.CreatedByUserId == userId && branchIds.Contains(o.BranchId));
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(o => o.Code.Contains(keyword));
        if (date.HasValue) query = query.Where(o => o.BusinessDate == date.Value);
        return query.Include(o => o.OrderItems).OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).Take(100).ToListAsync(ct);
    }

    public Task<SalesOrder?> GetDetailAsync(int id, int userId, CancellationToken ct) => db.SalesOrders.AsNoTracking()
        .Include(o => o.Branch).Include(o => o.OrderItems).ThenInclude(i => i.OrderItemToppings)
        .Include(o => o.OrderItems).ThenInclude(i => i.OrderItemNotes)
        .AsSplitQuery().SingleOrDefaultAsync(o => o.Id == id && o.CreatedByUserId == userId, ct);
}
