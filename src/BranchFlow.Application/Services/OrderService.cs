using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.Data.SqlClient;

namespace BranchFlow.Application.Services;

public sealed class OrderService(IOrderRepository orders, IOrderMenuService menus, IBranchAccessService access,
    IUnitOfWork work, TimeProvider clock) : IOrderService
{
    // Giá và tên snapshot luôn lấy từ menu DB; request không có trường giá do client quyết định.
    public static List<OrderItem> PriceItems(OrderMenu menu, CreateOrderRequest request)
    {
        if (request.Items is null || request.Items.Count is < 1 or > 100)
            throw new OrderValidationException("Đơn phải có từ 1 đến 100 dòng món.");
        List<OrderItem> items = [];
        foreach (var input in request.Items)
        {
            if (input is null || input.Quantity is < 1 or > 999 || input.Toppings is null || input.NoteOptionIds is null || input.Toppings.Count > 100 || input.NoteOptionIds.Count > 100)
                throw new OrderValidationException("Mỗi dòng phải có từ 1 đến 999 ly và danh sách topping/ghi chú hợp lệ.");
            if (input.ProductSizeId <= 0)
                throw new OrderValidationException("ProductSizeId phải là số nguyên dương.");
            var product = menu.Categories.SelectMany(c => c.Products).SingleOrDefault(p => p.ProductSizes.Any(s => s.Id == input.ProductSizeId))
                ?? throw new OrderValidationException("Món hoặc size không còn được bán tại chi nhánh.");
            var size = product.ProductSizes.Single(s => s.Id == input.ProductSizeId);
            var item = new OrderItem { ProductSizeId = size.Id, ProductNameSnapshot = product.Name, SizeNameSnapshot = size.SizeName,
                Quantity = input.Quantity, UnitPriceSnapshot = size.Price };
            decimal cupPrice = size.Price;
            HashSet<int> toppingIds = [];
            foreach (var selected in input.Toppings)
            {
                if (selected is null)
                    throw new OrderValidationException("Topping không được null.");
                if (selected.ToppingId <= 0)
                    throw new OrderValidationException("ToppingId phải là số nguyên dương.");
                if (selected.Quantity is < 1 or > 3 || !toppingIds.Add(selected.ToppingId))
                    throw new OrderValidationException("Mỗi topping chỉ xuất hiện một lần, từ 1 đến 3 phần trên mỗi ly.");
                var topping = menu.ToppingGroups.SelectMany(g => g.Toppings).SingleOrDefault(t => t.Id == selected.ToppingId)
                    ?? throw new OrderValidationException("Topping không còn được bán tại chi nhánh.");
                if (selected.Quantity > topping.MaxQuantity) throw new OrderValidationException("Vượt số phần topping được phép.");
                cupPrice = checked(cupPrice + topping.UnitPrice * selected.Quantity);
                item.OrderItemToppings.Add(new OrderItemTopping { ToppingId = topping.Id, Quantity = selected.Quantity,
                    ToppingNameSnapshot = topping.Name, UnitPriceSnapshot = topping.UnitPrice });
            }
            HashSet<int> noteIds = [], groups = [];
            foreach (var id in input.NoteOptionIds)
            {
                if (id <= 0)
                    throw new OrderValidationException("NoteOptionId phải là số nguyên dương.");

                var group = menu.NoteGroups.SingleOrDefault(g => g.Options.Any(o => o.Id == id));
                if (group is null || group.MaxSelections < 1 || !noteIds.Add(id) || !groups.Add(group.Id))
                    throw new OrderValidationException("Ghi chú không hợp lệ; mỗi nhóm được chọn tối đa một lựa chọn.");
                item.OrderItemNotes.Add(new OrderItemNote { NoteOptionId = id, NoteNameSnapshot = group.Options.Single(o => o.Id == id).Name });
            }
            item.SubtotalAmount = checked(cupPrice * input.Quantity);
            items.Add(item);
        }
        decimal total;

        try{
            total = items.Aggregate(0m,(current, item) => checked(current + item.SubtotalAmount));
        }
        catch (OverflowException){
            throw new OrderValidationException("Giá trị đơn vượt giới hạn.");
        }

        if (total < 0 || total > 9999999999999999.99m)
        {
            throw new OrderValidationException("Giá trị đơn vượt giới hạn.");
        }
        return items;
    }

    // Một transaction bao gồm cấp mã, kiểm tra menu, header, các dòng và snapshot tùy chọn.
    public async Task<CreatedOrder> CreateAsync(int userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        if (request.BranchId <= 0) throw new OrderValidationException("branchId phải là số nguyên dương.");
        if (!await access.HasBranchScopeAsync(userId, request.BranchId, ct)) throw new UnauthorizedAccessException();
        await work.BeginTransactionAsync(ct);
        try
        {
            var now = clock.GetUtcNow();
            var date = DateOnly.FromDateTime(now.ToOffset(TimeSpan.FromHours(7)).DateTime);
            var number = await orders.AllocateCodeAsync(request.BranchId, date, ct);
            var menu = await menus.GetMenuByBranchAsync(userId, request.BranchId, ct) ?? throw new OrderBranchUnavailableException();
            var items = PriceItems(menu, request);
            var total = items.Aggregate(0m,(current, item) => checked(current + item.SubtotalAmount));

            var order = new SalesOrder { Code = number.Code, DailySequence = number.Sequence, BranchId = request.BranchId,
                CreatedByUserId = userId, BusinessDate = date, CreatedAt = now.UtcDateTime, Status = "COMPLETED",
                TotalAmount = total, OrderItems = items };
            orders.Add(order);
            await work.SaveChangesAsync(ct);
            await work.CommitTransactionAsync(ct);
            return new CreatedOrder(order.Id, order.Code, order.BranchId, date, order.TotalAmount, order.Status, now);
        }
        catch (SqlException ex) when (ex.Number == 51001)
        {
            await work.RollbackTransactionAsync(CancellationToken.None);
            throw new OrderBranchUnavailableException();
        }
        catch
        {
            await work.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<List<OrderSummary>> GetMineAsync(int userId, string? keyword, DateOnly? date, CancellationToken ct = default)
    {
        if (keyword?.Length > 100) throw new OrderValidationException("Từ khóa quá dài.");
        var branches = await access.GetAccessibleBranchesAsync(userId, ct);
        var rows = await orders.GetMineAsync(userId, branches.Select(b => b.Id).ToList(), keyword?.Trim(), date, ct);
        return rows.Select(o => new OrderSummary(o.Id, o.Code, o.TotalAmount, o.OrderItems.Sum(i => i.Quantity), o.Status,
            new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc)))).ToList();
    }

    public async Task<OrderDetail?> GetDetailAsync(int userId, int id, CancellationToken ct = default)
    {
        var o = await orders.GetDetailAsync(id, userId, ct);
        if (o is null || !await access.CanAccessBranchAsync(userId, o.BranchId, ct)) return null;
        return new OrderDetail(o.Id, o.Code, o.BranchId, o.Branch.Code, o.Branch.Name, o.BusinessDate, o.TotalAmount, o.Status,
            o.ReportReason, o.ReportedAt.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(o.ReportedAt.Value, DateTimeKind.Utc)) : null,
            new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc)), o.OrderItems.OrderBy(i => i.Id).Select(i =>
                new OrderDetailItem(i.ProductNameSnapshot, i.SizeNameSnapshot, i.Quantity, i.UnitPriceSnapshot, i.SubtotalAmount,
                    i.OrderItemToppings.Select(t => new OrderDetailTopping(t.ToppingNameSnapshot, t.Quantity, t.UnitPriceSnapshot)).ToList(),
                    i.OrderItemNotes.Select(n => new OrderDetailNote(n.NoteNameSnapshot)).ToList())).ToList());
    }
}
