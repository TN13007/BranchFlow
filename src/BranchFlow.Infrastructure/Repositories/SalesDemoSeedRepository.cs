using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

public sealed class SalesDemoSeedRepository : ISalesDemoSeedRepository
{
    private readonly BranchFlowDbContext _context;

    public SalesDemoSeedRepository(BranchFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<Branch>> GetBranchesByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Where(x => codes.Contains(x.Code) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetCategoriesByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddCategories(IEnumerable<Category> categories)
    {
        _context.Categories.AddRange(categories);
    }

    public async Task<List<Product>> GetProductsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddProducts(IEnumerable<Product> products)
    {
        _context.Products.AddRange(products);
    }

    public async Task<List<Size>> GetSizesByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sizes
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddSizes(IEnumerable<Size> sizes)
    {
        _context.Sizes.AddRange(sizes);
    }

    public async Task<List<ProductSize>> GetProductSizesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductSizes
            .Where(x => productIds.Contains(x.ProductId) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddProductSizes(IEnumerable<ProductSize> productSizes)
    {
        _context.ProductSizes.AddRange(productSizes);
    }

    public async Task<List<ToppingGroup>> GetToppingGroupsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.ToppingGroups
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddToppingGroups(IEnumerable<ToppingGroup> groups)
    {
        _context.ToppingGroups.AddRange(groups);
    }

    public async Task<List<Topping>> GetToppingsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.Toppings
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddToppings(IEnumerable<Topping> toppings)
    {
        _context.Toppings.AddRange(toppings);
    }

    public async Task<List<NoteGroup>> GetNoteGroupsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        return await _context.NoteGroups
            .Where(x => names.Contains(x.Name) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddNoteGroups(IEnumerable<NoteGroup> groups)
    {
        _context.NoteGroups.AddRange(groups);
    }

    public async Task<List<NoteOption>> GetNoteOptionsAsync(
        IReadOnlyCollection<int> noteGroupIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.NoteOptions
            .Where(x => noteGroupIds.Contains(x.NoteGroupId) && !x.Deleted)
            .ToListAsync(cancellationToken);
    }

    public void AddNoteOptions(IEnumerable<NoteOption> options)
    {
        _context.NoteOptions.AddRange(options);
    }

    public async Task<List<BranchProduct>> GetBranchProductsAsync(
        IReadOnlyCollection<int> branchIds,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.BranchProducts
            .Where(x =>
                branchIds.Contains(x.BranchId) &&
                productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);
    }

    public void AddBranchProducts(IEnumerable<BranchProduct> branchProducts)
    {
        _context.BranchProducts.AddRange(branchProducts);
    }

    public async Task<List<BranchTopping>> GetBranchToppingsAsync(
        IReadOnlyCollection<int> branchIds,
        IReadOnlyCollection<int> toppingIds,
        CancellationToken cancellationToken = default)
    {
        return await _context.BranchToppings
            .Where(x =>
                branchIds.Contains(x.BranchId) &&
                toppingIds.Contains(x.ToppingId))
            .ToListAsync(cancellationToken);
    }

    public void AddBranchToppings(IEnumerable<BranchTopping> branchToppings)
    {
        _context.BranchToppings.AddRange(branchToppings);
    }
}