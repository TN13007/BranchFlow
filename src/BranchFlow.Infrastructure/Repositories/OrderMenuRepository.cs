
using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class OrderMenuRepository : IOrderMenuRepository
{
    private readonly BranchFlowDbContext _context;

    public OrderMenuRepository(BranchFlowDbContext context)
    {
        _context = context;
    }

    public async Task<OrderMenuData?> GetMenuByBranchAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        // 1. Get branch information.
        var branch = await _context.Branches
            .AsNoTracking()
            .Where(b =>
                b.Id == branchId &&
                b.IsActive &&
                !b.Deleted)
            .Select(b => new OrderMenuBranchData(
                b.Id,
                b.Code,
                b.Name))
            .FirstOrDefaultAsync(cancellationToken);
        //Nếu branch không tồn tại, bị xóa hoặc inactive, Repository trả về null. Tầng xử lý phía trên sẽ chuyển kết quả này thành HTTP 404 theo contract của SCRUM-76.
        if (branch is null)
        {
            return null;
        }

        // 2. Get available products and their valid sizes.
        var productRows = await _context.BranchProducts
            .AsNoTracking()
            .Where(bp =>
                bp.BranchId == branchId &&
                bp.IsAvailable &&
                bp.Product.IsActive &&
                !bp.Product.Deleted &&
                bp.Product.Category.IsActive &&
                !bp.Product.Category.Deleted)
            .SelectMany(
                bp => bp.Product.ProductSizes.Where(ps =>
                    ps.IsActive &&
                    !ps.Deleted &&
                    ps.Size.IsActive &&
                    !ps.Size.Deleted),
                (bp, ps) => new
                {
                    CategoryId = bp.Product.Category.Id,
                    CategoryName = bp.Product.Category.Name,
                    ProductId = bp.Product.Id,
                    ProductName = bp.Product.Name,
                    bp.Product.ImageUrl,
                    ProductSizeId = ps.Id,
                    ps.SizeId,
                    SizeName = ps.Size.Name,
                    ps.Price
                })
            .ToListAsync(cancellationToken);

        var categories = productRows
            .GroupBy(p => new
            {
                p.CategoryId,
                p.CategoryName
            })
            .Select(categoryGroup =>
                new OrderMenuCategoryData(
                    categoryGroup.Key.CategoryId,
                    categoryGroup.Key.CategoryName,
                    categoryGroup
                        .GroupBy(p => new
                        {
                            p.ProductId,
                            p.ProductName,
                            p.ImageUrl
                        })
                        .Select(productGroup =>
                        {
                            var sizes = productGroup
                                .Select(p => new OrderMenuProductSizeData(
                                    p.ProductSizeId,
                                    p.SizeId,
                                    p.SizeName,
                                    p.Price))
                                .OrderBy(s => s.Price)
                                .ToList();

                            return new OrderMenuProductData(
                                productGroup.Key.ProductId,
                                productGroup.Key.ProductName,
                                productGroup.Key.ImageUrl,
                                sizes.Min(s => s.Price),
                                sizes);
                        })
                        .OrderBy(p => p.Name)
                        .ToList()))
            .OrderBy(c => c.Name)
            .ToList();

        // 3. Get available topping groups and toppings.
        var toppingRows = await _context.BranchToppings
            .AsNoTracking()
            .Where(bt =>
                bt.BranchId == branchId &&
                bt.IsAvailable &&
                bt.Topping.IsActive &&
                !bt.Topping.Deleted &&
                bt.Topping.ToppingGroup.IsActive &&
                !bt.Topping.ToppingGroup.Deleted)
            .Select(bt => new
            {
                GroupId = bt.Topping.ToppingGroup.Id,
                GroupName = bt.Topping.ToppingGroup.Name,
                ToppingId = bt.Topping.Id,
                ToppingName = bt.Topping.Name,
                UnitPrice = bt.Topping.Price
            })
            .ToListAsync(cancellationToken);

        var toppingGroups = toppingRows
            .GroupBy(t => new
            {
                t.GroupId,
                t.GroupName
            })
            .Select(group => new OrderMenuToppingGroupData(
                group.Key.GroupId,
                group.Key.GroupName,
                group.Select(t => new OrderMenuToppingData(
                    t.ToppingId,
                    t.ToppingName,
                    t.UnitPrice,
                    3))
                .OrderBy(t => t.Name)
                .ToList()))
            .OrderBy(g => g.Name)
            .ToList();

        // 4. Get active note groups and their active options.
        var noteRows = await _context.NoteGroups
            .AsNoTracking()
            .Where(ng =>
                ng.IsActive &&
                !ng.Deleted &&
                ng.NoteOptions.Any(no =>
                    no.IsActive &&
                    !no.Deleted))
            .SelectMany(
                ng => ng.NoteOptions.Where(no =>
                    no.IsActive &&
                    !no.Deleted),
                (ng, no) => new
                {
                    GroupId = ng.Id,
                    GroupName = ng.Name,
                    OptionId = no.Id,
                    OptionName = no.Name
                })
            .ToListAsync(cancellationToken);

        var noteGroups = noteRows
            .GroupBy(n => new
            {
                n.GroupId,
                n.GroupName
            })
            .Select(group => new OrderMenuNoteGroupData(
                group.Key.GroupId,
                group.Key.GroupName,
                1,
                group.Select(n => new OrderMenuNoteOptionData(
                    n.OptionId,
                    n.OptionName))
                .OrderBy(o => o.Name)
                .ToList()))
            .OrderBy(g => g.Name)
            .ToList();

        // 5. Return the complete menu data.
        return new OrderMenuData(
            branch,
            categories,
            toppingGroups,
            noteGroups);
    }
}