using System.Text.Json.Serialization;

namespace BranchFlow.Application.DTOs.Sprint4;

public sealed record LoginRequest(string Username, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed class TokenPair
{
    public string AccessToken { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
}
public sealed record CurrentUser(int UserId, string Username, string Role);
public sealed record AccessibleBranch(int Id, string Code, string Name);
public sealed record OrderMenu(AccessibleBranch Branch, List<MenuCategory> Categories,
    List<MenuToppingGroup> ToppingGroups, List<MenuNoteGroup> NoteGroups);
public sealed record MenuCategory(int Id, string Name, List<MenuProduct> Products);
public sealed record MenuProduct(int Id, string Name, string? ImageUrl, decimal MinPrice, List<MenuProductSize> ProductSizes);
public sealed record MenuProductSize(int Id, int SizeId, string SizeName, decimal Price);
public sealed record MenuToppingGroup(int Id, string Name, List<MenuTopping> Toppings);
public sealed record MenuTopping(int Id, string Name, decimal UnitPrice, int MaxQuantity);
public sealed record MenuNoteGroup(int Id, string Name, int MaxSelections, List<MenuNoteOption> Options);
public sealed record MenuNoteOption(int Id, string Name);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateOrderRequest(int BranchId, List<CreateOrderItem> Items);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateOrderItem(int ProductSizeId, int Quantity, List<CreateOrderTopping> Toppings, List<int> NoteOptionIds);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateOrderTopping(int ToppingId, int Quantity);

public sealed record CreatedOrder(int Id, string Code, int BranchId, DateOnly BusinessDate,
    decimal TotalAmount, string Status, DateTimeOffset CreatedAt);
public sealed record OrderSummary(int Id, string Code, decimal TotalAmount, int TotalQuantity,
    string Status, DateTimeOffset CreatedAt);
public sealed record OrderDetail(int Id, string Code, int BranchId, string BranchCode, string BranchName,
    DateOnly BusinessDate, decimal TotalAmount, string Status, string? ReportReason,
    DateTimeOffset? ReportedAt, DateTimeOffset CreatedAt, List<OrderDetailItem> Items);
public sealed record OrderDetailItem(string ProductName, string SizeName, int Quantity, decimal UnitPrice,
    decimal SubtotalAmount, List<OrderDetailTopping> Toppings, List<OrderDetailNote> Notes);
public sealed record OrderDetailTopping(string Name, int Quantity, decimal UnitPrice);
public sealed record OrderDetailNote(string Name);
