
namespace BranchFlow.Infrastructure.Repositories;

// Root data returned by the repository.
public sealed record OrderMenuData(
    OrderMenuBranchData Branch,
    List<OrderMenuCategoryData> Categories,
    List<OrderMenuToppingGroupData> ToppingGroups,
    List<OrderMenuNoteGroupData> NoteGroups
);

// Branch information.
public sealed record OrderMenuBranchData(
    int Id,
    string Code,
    string Name
);

// Category and products.
public sealed record OrderMenuCategoryData(
    int Id,
    string Name,
    List<OrderMenuProductData> Products
);

// Product information.
public sealed record OrderMenuProductData(
    int Id,
    string Name,
    string? ImageUrl,
    decimal MinPrice,
    List<OrderMenuProductSizeData> ProductSizes
);

// Product size and its selling price.
public sealed record OrderMenuProductSizeData(
    int Id,
    int SizeId,
    string SizeName,
    decimal Price
);

// Topping groups.
public sealed record OrderMenuToppingGroupData(
    int Id,
    string Name,
    List<OrderMenuToppingData> Toppings
);

// Topping information.
public sealed record OrderMenuToppingData(
    int Id,
    string Name,
    decimal UnitPrice,
    int MaxQuantity
);

// Note groups.
public sealed record OrderMenuNoteGroupData(
    int Id,
    string Name,
    int MaxSelections,
    List<OrderMenuNoteOptionData> Options
);

// Note options.
public sealed record OrderMenuNoteOptionData(
    int Id,
    string Name
);
