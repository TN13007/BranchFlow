namespace BranchFlow.Application.DTOs.Mvp;

public sealed class NamedOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
public sealed class BranchView
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public int? AdminUserId { get; set; }
    public string AdminName { get; set; } = "";
}
public sealed record EmployeeDirectoryEntry(string EmployeeCode, string FullName, string? Position,
    int BranchId, string BranchName, bool IsActive);

public sealed class UserView
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "EMPLOYEE";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public string EmployeeCode { get; set; } = "";
    public string? Position { get; set; }
    public int? BranchId { get; set; }
    public string BranchName { get; set; } = "";
    public bool CanReceive { get; set; }
    public bool CanIssue { get; set; }
}
public sealed class CreateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "EMPLOYEE";
    public int? BranchId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
public sealed class PriceView
{
    public int Id { get; set; }
    public int SizeId { get; set; }
    public string SizeName { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class ProductView
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public List<PriceView> Prices { get; set; } = [];
}
public sealed class ToppingView
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
}
public sealed class NoteView
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
public sealed class IngredientView
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int UnitId { get; set; }
    public string UnitName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
public sealed class BalanceView
{
    public int IngredientId { get; set; }
    public bool IsActive { get; set; } = true;
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal WarningThreshold { get; set; }
    public string RowVersion { get; set; } = "";
}
public sealed record StockCapabilities(bool CanReceive, bool CanIssue);

public sealed class StockLineRequest
{
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public string RowVersion { get; set; } = "";
}
public sealed class StockWriteRequest
{
    public int BranchId { get; set; }
    public string Type { get; set; } = "IN";
    public string Reason { get; set; } = "";
    public List<StockLineRequest> Lines { get; set; } = [];
}
public sealed class StockHistoryView
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string PerformedBy { get; set; } = "";
    public List<StockHistoryLine> Lines { get; set; } = [];
}
public sealed class StockHistoryLine
{
    public string Name { get; set; } = "";
    public decimal Change { get; set; }
    public decimal Before { get; set; }
    public decimal After { get; set; }
}
public sealed class ManagedOrderView
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Code { get; set; } = "";
    public string BranchName { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RowVersion { get; set; } = "";
    public List<ManagedOrderLine> Items { get; set; } = [];
    public List<AdjustmentView> History { get; set; } = [];
}
public sealed class ManagedOrderLine
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Size { get; set; } = "";
    public string Options { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public decimal Subtotal { get; set; }
}
public sealed class AdjustmentView
{
    public DateTime CreatedAt { get; set; }
    public string PerformedBy { get; set; } = "";
    public string Reason { get; set; } = "";
    public string BeforeData { get; set; } = "";
    public string AfterData { get; set; } = "";
}
public sealed class AdjustOrderRequest
{
    public string Reason { get; set; } = "";
    public string RowVersion { get; set; } = "";
    public bool Cancel { get; set; }
    public List<QuantityChange> Items { get; set; } = [];
}
public sealed class QuantityChange
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}
public sealed class RevenueView
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = "";
    public DateOnly Date { get; set; }
    public int Orders { get; set; }
    public decimal Amount { get; set; }
}
public sealed class AvailabilityRequest { public bool IsAvailable { get; set; } }
public sealed class AssignmentRequest { public int UserId { get; set; } public int BranchId { get; set; } }
public sealed class ThresholdRequest { public decimal WarningThreshold { get; set; } public string RowVersion { get; set; } = ""; }
