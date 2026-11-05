using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Repositories;

namespace BranchFlow.Application.Services;

public sealed class OrderMenuService : IOrderMenuService
{
    private readonly IOrderMenuRepository _orderMenuRepository;
    private readonly IBranchAccessService _branchAccess;

    public OrderMenuService(IOrderMenuRepository orderMenuRepository, IBranchAccessService branchAccess)
    {
        _orderMenuRepository = orderMenuRepository;
        _branchAccess = branchAccess;
    }

    public async Task<OrderMenu?> GetMenuByBranchAsync(
        int userId,
        int branchId,
        CancellationToken cancellationToken = default)
    {
        if (branchId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(branchId),
                "Branch ID must be greater than zero.");
        }

        // Kiểm tra phạm vi trước khi đọc menu, không tin branchId do client gửi.
        if (!await _branchAccess.HasBranchScopeAsync(userId, branchId, cancellationToken))
            throw new UnauthorizedAccessException("Bạn không được phân công tại chi nhánh này.");

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
