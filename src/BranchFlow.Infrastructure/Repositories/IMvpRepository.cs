using BranchFlow.Infrastructure.Models;

namespace BranchFlow.Infrastructure.Repositories;

public interface IMvpRepository
{
    Task<AppUser?> UserAsync(int id, CancellationToken ct);
    Task<Role?> RoleAsync(string code, CancellationToken ct);
    Task<List<AppUser>> UsersAsync(CancellationToken ct);
    Task<List<Branch>> BranchesAsync(CancellationToken ct);
    Task<Branch?> BranchAsync(int id, CancellationToken ct);
    Task<List<AdminBranchAssignment>> AdminAssignmentsAsync(CancellationToken ct);
    Task<List<EmployeeBranchAssignment>> EmployeeAssignmentsAsync(CancellationToken ct);
    Task<List<EmployeeStockPermission>> StockPermissionsAsync(CancellationToken ct);
    Task<List<Category>> CategoriesAsync(CancellationToken ct);
    Task<List<Size>> SizesAsync(CancellationToken ct);
    Task<List<ToppingGroup>> ToppingGroupsAsync(CancellationToken ct);
    Task<List<NoteGroup>> NoteGroupsAsync(CancellationToken ct);
    Task<List<IngredientUnit>> UnitsAsync(CancellationToken ct);
    Task<List<Product>> ProductsAsync(CancellationToken ct);
    Task<List<Topping>> ToppingsAsync(CancellationToken ct);
    Task<List<BranchProduct>> BranchProductsAsync(int branchId, CancellationToken ct);
    Task<List<BranchTopping>> BranchToppingsAsync(int branchId, CancellationToken ct);
    Task<List<Ingredient>> IngredientsAsync(CancellationToken ct);
    Task<List<BranchIngredient>> BalancesAsync(int branchId, CancellationToken ct);
    Task<List<StockTransaction>> StockHistoryAsync(int branchId, CancellationToken ct);
    Task<List<SalesOrder>> OrdersAsync(int[] branchIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
    Task<SalesOrder?> OrderAsync(int id, CancellationToken ct);
    Task<List<RevenueData>> RevenueAsync(int[] branchIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
    void Add(object entity);
    void ExpectVersion(BranchIngredient entity, byte[] version);
    void ExpectVersion(SalesOrder entity, byte[] version);
}
public sealed class RevenueData
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = "";
    public DateTime VietnamDate { get; set; }
    public int Orders { get; set; }
    public decimal Amount { get; set; }
}
