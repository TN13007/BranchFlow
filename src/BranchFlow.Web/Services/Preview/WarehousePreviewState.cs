using BranchFlow.Web.Models.Inventory;

namespace BranchFlow.Web.Services.Preview;

// Development fixture source. A future API adapter can supply snapshots/updates here;
// there is deliberately no guessed stock endpoint or write request.
public sealed class WarehousePreviewState
{
    public event Action? Changed;
    public IReadOnlyList<WarehouseStock> Warehouses { get; private set; } = CreatePreview();

    public void UpdateStock(string warehouseId, string ingredientId, decimal quantity, StockAvailability? availability = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        var warehouse = Warehouses.FirstOrDefault(item => item.Id == warehouseId)
            ?? throw new ArgumentException("Unknown warehouse.", nameof(warehouseId));
        if (!warehouse.Items.Any(item => item.Id == ingredientId)) throw new ArgumentException("Unknown ingredient.", nameof(ingredientId));
        ReplaceItems(warehouseId, warehouse.Items.Select(item => item.Id == ingredientId
            ? item with { Quantity = quantity, Availability = availability ?? item.Availability } : item));
    }

    public void ReplaceItems(string warehouseId, IEnumerable<StockIngredient> ingredients)
    {
        if (!Warehouses.Any(item => item.Id == warehouseId)) throw new ArgumentException("Unknown warehouse.", nameof(warehouseId));
        var items = ingredients.ToArray();
        if (items.Any(item => item.Quantity < 0)) throw new ArgumentOutOfRangeException(nameof(ingredients));
        if (items.Select(item => item.Id).Distinct().Count() != items.Length) throw new ArgumentException("Duplicate ingredient IDs.", nameof(ingredients));
        Warehouses = Array.AsReadOnly(Warehouses.Select(warehouse => warehouse.Id == warehouseId
            ? warehouse with { Items = Array.AsReadOnly(items) } : warehouse).ToArray());
        Changed?.Invoke();
    }

    private static IReadOnlyList<WarehouseStock> CreatePreview()
    {
        StockIngredient[] items =
        [
            new("ING-01", "ING-01", "Trà đào cam sả (Cốt trà)", 15m, "lít", "/images/christmas/stock/products/peach-tea.webp", StockAvailability.Available),
            new("ING-02", "ING-02", "Trân châu đường đen", 4.5m, "kg", "/images/christmas/stock/products/tapioca.webp", StockAvailability.Available),
            new("ING-03", "ING-03", "Bột Matcha Nhật Bản", 1.2m, "kg", "/images/christmas/stock/products/matcha.webp", StockAvailability.NeedsReview)
        ];
        return Array.AsReadOnly(Enumerable.Range(1, 5).Select(index => new WarehouseStock($"warehouse-{index}", $"Kho {index:00}",
            index == 1 ? Array.AsReadOnly(items) : Array.Empty<StockIngredient>())).ToArray());
    }
}
