using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Web.Models.Cart;

public sealed record CartLine(Guid Key, string ProductName, string SizeName, string? ImageUrl,
    CreateOrderItem Request, decimal PreviewUnitPrice, string Description)
{
    public decimal PreviewTotal => checked(PreviewUnitPrice * Request.Quantity);
}

public sealed class CartState
{
    private readonly List<CartLine> lines = [];
    public IReadOnlyList<CartLine> Lines => lines;
    public int? BranchId { get; private set; }
    public bool OutcomeUnknown { get; private set; }
    public CreatedOrder? LastCreated { get; private set; }
    public decimal PreviewTotal => lines.Sum(l => l.PreviewTotal);
    public long TotalQuantity => lines.Sum(l => (long)l.Request.Quantity);

    public void SetBranch(int branchId)
    {
        if (BranchId != branchId)
        {
            if (OutcomeUnknown) throw new InvalidOperationException("Cần kiểm tra đơn trước khi đổi chi nhánh.");
            lines.Clear();
            BranchId = branchId;
        }
    }

    public void Save(OrderMenu menu, CreateOrderItem request, Guid? existing = null)
    {
        if (OutcomeUnknown) throw new InvalidOperationException("Cần kiểm tra kết quả đơn trước khi sửa giỏ.");
        if (BranchId != menu.Branch.Id) throw new InvalidOperationException("Giỏ không thuộc chi nhánh hiện tại.");
        var line = BuildLine(menu, request, existing ?? Guid.NewGuid());
        var index = existing.HasValue ? lines.FindIndex(l => l.Key == existing) : -1;
        if (existing.HasValue && index < 0) throw new InvalidOperationException("Dòng giỏ không còn tồn tại.");
        if (index >= 0) lines[index] = line; else lines.Add(line);
    }

    public static CartLine BuildLine(OrderMenu menu, CreateOrderItem request, Guid key)
    {
        if (request.ProductSizeId <= 0) throw new InvalidOperationException("Vui lòng chọn size trước khi thêm món.");
        if (request.Quantity <= 0) throw new InvalidOperationException("Số ly phải là số nguyên dương.");
        var product = menu.Categories.SelectMany(c => c.Products).SingleOrDefault(p => p.ProductSizes.Any(s => s.Id == request.ProductSizeId))
            ?? throw new InvalidOperationException("Món hoặc size không còn được bán. Vui lòng sửa giỏ.");
        var size = product.ProductSizes.Single(s => s.Id == request.ProductSizeId);
        if (request.Toppings is null || request.NoteOptionIds is null ||
            request.Toppings.Any(t => t is null || t.ToppingId <= 0 || t.Quantity is < 1 or > 3) ||
            request.Toppings.Select(t => t.ToppingId).Distinct().Count() != request.Toppings.Count ||
            request.NoteOptionIds.Any(id => id <= 0) || request.NoteOptionIds.Distinct().Count() != request.NoteOptionIds.Count)
            throw new InvalidOperationException("Topping/ghi chú không hợp lệ hoặc bị trùng.");
        decimal unitPrice = size.Price;
        List<string> labels = [];
        foreach (var selected in request.Toppings)
        {
            var topping = menu.ToppingGroups.SelectMany(g => g.Toppings).SingleOrDefault(t => t.Id == selected.ToppingId)
                ?? throw new InvalidOperationException("Topping không còn được bán. Vui lòng sửa giỏ.");
            if (selected.Quantity > Math.Min(3, topping.MaxQuantity)) throw new InvalidOperationException("Vượt số phần topping được phép.");
            unitPrice = checked(unitPrice + topping.UnitPrice * selected.Quantity);
            labels.Add($"{topping.Name} × {selected.Quantity}");
        }
        HashSet<int> groups = [];
        foreach (var id in request.NoteOptionIds)
        {
            var group = menu.NoteGroups.SingleOrDefault(g => g.Options.Any(o => o.Id == id));
            if (group is null || group.MaxSelections < 1 || !groups.Add(group.Id))
                throw new InvalidOperationException("Mỗi nhóm ghi chú chỉ được chọn một lựa chọn hợp lệ.");
            labels.Add(group.Options.Single(o => o.Id == id).Name);
        }
        if (unitPrice < 0 || checked(unitPrice * request.Quantity) > 9999999999999999.99m)
            throw new InvalidOperationException("Giá trị đơn vượt giới hạn cho phép.");
        return new CartLine(key, product.Name, size.SizeName, product.ImageUrl,
            request with { Toppings = request.Toppings.ToList(), NoteOptionIds = request.NoteOptionIds.ToList() }, unitPrice, string.Join(" · ", labels));
    }

    public void Revalidate(OrderMenu menu)
    {
        if (BranchId != menu.Branch.Id) throw new InvalidOperationException("Chi nhánh menu không khớp giỏ.");
        var updated = lines.Select(l => BuildLine(menu, l.Request, l.Key)).ToList();
        lines.Clear(); lines.AddRange(updated);
    }

    public CreateOrderRequest ToRequest(OrderMenu menu)
    {
        if (OutcomeUnknown) throw new InvalidOperationException("Hãy kiểm tra danh sách đơn trước khi xác nhận lại.");
        if (lines.Count == 0) throw new InvalidOperationException("Vui lòng chọn ít nhất một món.");
        Revalidate(menu);
        return new CreateOrderRequest(menu.Branch.Id, lines.Select(l => l.Request with
        { Toppings = l.Request.Toppings.ToList(), NoteOptionIds = l.Request.NoteOptionIds.ToList() }).ToList());
    }

    public void Remove(Guid key)
    {
        if (!OutcomeUnknown) lines.RemoveAll(l => l.Key == key);
    }
    public void MarkUnknown() => OutcomeUnknown = true;
    public void Complete(CreatedOrder result) { LastCreated = result; Clear(); }
    public void AcknowledgeCheckedHistory() => OutcomeUnknown = false;
    public void Clear() { lines.Clear(); OutcomeUnknown = false; }
}
