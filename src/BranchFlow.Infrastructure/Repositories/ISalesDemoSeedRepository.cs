
using BranchFlow.Infrastructure.Models;

namespace BranchFlow.Infrastructure.Repositories;

public interface ISalesDemoSeedRepository
{
    // Branches
    Task<List<Branch>> GetBranchesByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken = default);

    // Categories
    Task<List<Category>> GetCategoriesByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddCategories(IEnumerable<Category> categories);

    // Products
    Task<List<Product>> GetProductsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddProducts(IEnumerable<Product> products);

    // Sizes
    Task<List<Size>> GetSizesByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddSizes(IEnumerable<Size> sizes);

    // Product sizes and prices
    Task<List<ProductSize>> GetProductSizesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    void AddProductSizes(IEnumerable<ProductSize> productSizes);

    // Topping groups
    Task<List<ToppingGroup>> GetToppingGroupsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddToppingGroups(IEnumerable<ToppingGroup> groups);

    // Toppings
    Task<List<Topping>> GetToppingsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddToppings(IEnumerable<Topping> toppings);

    // Note groups
    Task<List<NoteGroup>> GetNoteGroupsByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);

    void AddNoteGroups(IEnumerable<NoteGroup> groups);

    // Note options
    Task<List<NoteOption>> GetNoteOptionsAsync(
        IReadOnlyCollection<int> noteGroupIds,
        CancellationToken cancellationToken = default);

    void AddNoteOptions(IEnumerable<NoteOption> options);

    // Branch availability
    Task<List<BranchProduct>> GetBranchProductsAsync(
        IReadOnlyCollection<int> branchIds,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    void AddBranchProducts(IEnumerable<BranchProduct> branchProducts);

    Task<List<BranchTopping>> GetBranchToppingsAsync(
        IReadOnlyCollection<int> branchIds,
        IReadOnlyCollection<int> toppingIds,
        CancellationToken cancellationToken = default);

    void AddBranchToppings(IEnumerable<BranchTopping> branchToppings);
}