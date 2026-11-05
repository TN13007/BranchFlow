using System.Globalization;

namespace BranchFlow.Web.Models.Inventory;

public enum StockAvailability { Available, NeedsReview, Unavailable }

public sealed record StockIngredient(string Id, string Code, string Name, decimal Quantity, string Unit,
    string ImageUrl, StockAvailability Availability)
{
    public string QuantityText => $"{Quantity.ToString("0.0##", CultureInfo.GetCultureInfo("vi-VN"))} {Unit}";
    public string StatusLabel => Availability switch
    {
        StockAvailability.NeedsReview => "Lưu ý định lượng",
        StockAvailability.Unavailable => "Chưa sẵn sàng",
        _ => "Sẵn sàng"
    };
    public string StatusSymbol => Availability == StockAvailability.Available ? "✓" : "!";
}

public sealed record WarehouseStock(string Id, string Name, IReadOnlyList<StockIngredient> Items);
