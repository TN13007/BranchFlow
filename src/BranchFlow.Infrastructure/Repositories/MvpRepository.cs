using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Infrastructure.Repositories;

// Truy vấn dùng chung DbContext/UoW; không tự commit ở repository.
public sealed class MvpRepository(BranchFlowDbContext db) : IMvpRepository
{
    public Task<AppUser?> UserAsync(int id, CancellationToken ct) => db.AppUsers.Include(x => x.Role).Include(x => x.EmployeeProfile).SingleOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
    public Task<Role?> RoleAsync(string code, CancellationToken ct) => db.Roles.SingleOrDefaultAsync(x => x.Code == code && !x.Deleted, ct);
    public Task<List<AppUser>> UsersAsync(CancellationToken ct) => db.AppUsers.Include(x => x.Role).Include(x => x.EmployeeProfile).Where(x => !x.Deleted).OrderBy(x => x.FullName).ToListAsync(ct);
    public Task<List<Branch>> BranchesAsync(CancellationToken ct) => db.Branches.Where(x => !x.Deleted).OrderBy(x => x.Code).ToListAsync(ct);
    public Task<Branch?> BranchAsync(int id, CancellationToken ct) => db.Branches.SingleOrDefaultAsync(x => x.Id == id && !x.Deleted, ct);
    public Task<List<AdminBranchAssignment>> AdminAssignmentsAsync(CancellationToken ct) => db.AdminBranchAssignments.Include(x => x.AdminUser).ToListAsync(ct);
    public Task<List<EmployeeBranchAssignment>> EmployeeAssignmentsAsync(CancellationToken ct) => db.EmployeeBranchAssignments.Include(x => x.Branch).ToListAsync(ct);
    public Task<List<EmployeeStockPermission>> StockPermissionsAsync(CancellationToken ct) => db.EmployeeStockPermissions.ToListAsync(ct);
    public Task<List<Category>> CategoriesAsync(CancellationToken ct) => db.Categories.Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<Size>> SizesAsync(CancellationToken ct) => db.Sizes.Where(x => !x.Deleted).OrderBy(x => x.Id).ToListAsync(ct);
    public Task<List<ToppingGroup>> ToppingGroupsAsync(CancellationToken ct) => db.ToppingGroups.Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<NoteGroup>> NoteGroupsAsync(CancellationToken ct) => db.NoteGroups.Include(x => x.NoteOptions).Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<IngredientUnit>> UnitsAsync(CancellationToken ct) => db.IngredientUnits.Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<Product>> ProductsAsync(CancellationToken ct) => db.Products.Include(x => x.Category).Include(x => x.ProductSizes.Where(p => !p.Deleted)).ThenInclude(x => x.Size).Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<Topping>> ToppingsAsync(CancellationToken ct) => db.Toppings.Include(x => x.ToppingGroup).Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<BranchProduct>> BranchProductsAsync(int branchId, CancellationToken ct) => db.BranchProducts.Where(x => x.BranchId == branchId).ToListAsync(ct);
    public Task<List<BranchTopping>> BranchToppingsAsync(int branchId, CancellationToken ct) => db.BranchToppings.Where(x => x.BranchId == branchId).ToListAsync(ct);
    public Task<List<Ingredient>> IngredientsAsync(CancellationToken ct) => db.Ingredients.Include(x => x.Unit).Where(x => !x.Deleted).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<List<BranchIngredient>> BalancesAsync(int branchId, CancellationToken ct) => db.BranchIngredients.Include(x => x.Ingredient).ThenInclude(x => x.Unit).Where(x => x.BranchId == branchId).ToListAsync(ct);
    public Task<List<StockTransaction>> StockHistoryAsync(int branchId, CancellationToken ct) => db.StockTransactions.AsNoTracking().Include(x => x.PerformedByUser).Include(x => x.StockTransactionDetails).ThenInclude(x => x.Ingredient).Where(x => x.BranchId == branchId).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(100).ToListAsync(ct);
    public Task<List<SalesOrder>> OrdersAsync(int[] branchIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct) => db.SalesOrders.AsNoTracking().Include(x => x.Branch).Include(x => x.CreatedByUser).Where(x => branchIds.Contains(x.BranchId) && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(200).ToListAsync(ct);
    public Task<SalesOrder?> OrderAsync(int id, CancellationToken ct) => db.SalesOrders.Include(x => x.Branch).Include(x => x.CreatedByUser).Include(x => x.OrderItems).ThenInclude(x => x.OrderItemToppings).Include(x => x.OrderItems).ThenInclude(x => x.OrderItemNotes).Include(x => x.OrderAdjustments).ThenInclude(x => x.AdjustedByUser).AsSplitQuery().SingleOrDefaultAsync(x => x.Id == id, ct);
    // Tổng hợp trong SQL trước khi trả về; không tính từ danh sách 200 đơn trên UI.
    public Task<List<RevenueData>> RevenueAsync(int[] branchIds, DateTime fromUtc, DateTime toUtc, CancellationToken ct) => db.SalesOrders.AsNoTracking().Where(x => branchIds.Contains(x.BranchId) && x.Status == "COMPLETED" && x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
        .GroupBy(x => new { x.BranchId, x.Branch.Name, Date = x.CreatedAt.AddHours(7).Date })
        .Select(g => new RevenueData { BranchId = g.Key.BranchId, BranchName = g.Key.Name, VietnamDate = g.Key.Date, Orders = g.Count(), Amount = g.Sum(x => x.TotalAmount) }).OrderBy(x => x.VietnamDate).ThenBy(x => x.BranchId).ToListAsync(ct);
    public void Add(object entity) => db.Add(entity);
    public void ExpectVersion(BranchIngredient entity, byte[] version) => db.Entry(entity).Property(x => x.RowVersion).OriginalValue = version;
    public void ExpectVersion(SalesOrder entity, byte[] version) => db.Entry(entity).Property(x => x.RowVersion).OriginalValue = version;
}
