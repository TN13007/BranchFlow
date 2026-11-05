namespace BranchFlow.Web.Models.Inventory;

// View model for the stock API adapter; a missing quantity is never rendered as zero.
public sealed record InventoryBalance(string Id, string Code, string Name, string Unit,
    decimal Quantity, decimal? AlertThreshold, DateTimeOffset? UpdatedAt)
{
    public bool IsLow => AlertThreshold.HasValue && Quantity <= AlertThreshold.Value;
}
