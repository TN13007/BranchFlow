using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Repositories;

namespace BranchFlow.Application.Services;

public sealed class OrderMenuService : IOrderMenuService
{
    private readonly IOrderMenuRepository _orderMenuRepository;

    public OrderMenuService(IOrderMenuRepository orderMenuRepository)
    {
        _orderMenuRepository = orderMenuRepository;
    }

    public async Task<OrderMenu?> GetMenuByBranchAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch ID must be greater than zero.");
        }

        var menuData = await _orderMenuRepository.GetMenuByBranchAsync(
            branchId,
            cancellationToken);

        if (menuData is null)
        {
            return null;
        }

        var branch = new AccessibleBranch(
            menuData.Branch.Id,
            menuData.Branch.Code,
            menuData.Branch.Name);

        var categories = menuData.Categories
            .Select(category => new MenuCategory(
                category.Id,
                category.Name,
                category.Products
                    .Select(product => new MenuProduct(
                        product.Id,
                        product.Name,
                        product.ImageUrl,
                        product.MinPrice,
                        product.ProductSizes
                            .Select(size => new MenuProductSize(
                                size.Id,
                                size.SizeId,
                                size.SizeName,
                                size.Price))
                            .ToList()))
                    .ToList()))
            .ToList();

        var toppingGroups = menuData.ToppingGroups
            .Select(group => new MenuToppingGroup(
                group.Id,
                group.Name,
                group.Toppings
                    .Select(topping => new MenuTopping(
                        topping.Id,
                        topping.Name,
                        topping.UnitPrice,
                        topping.MaxQuantity))
                    .ToList()))
            .ToList();

        var noteGroups = menuData.NoteGroups
            .Select(group => new MenuNoteGroup(
                group.Id,
                group.Name,
                group.MaxSelections,
                group.Options
                    .Select(option => new MenuNoteOption(
                        option.Id,
                        option.Name))
                    .ToList()))
            .ToList();

        return new OrderMenu(
            branch,
            categories,
            toppingGroups,
            noteGroups);
    }
}