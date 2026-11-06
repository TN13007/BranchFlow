using System.Text.Json;
using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace BranchFlow.Application.Services;

public sealed class MvpService(IMvpRepository repo, IUnitOfWork work, IBranchAccessService access,
    IPasswordHasher<AppUser> hasher, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private static string Required(string? value, int max) =>
        value?.Trim() is { Length: > 0 } s && s.Length <= max ? s : throw new ArgumentException($"Nhập nội dung từ 1 đến {max} ký tự.");
    private static string? Optional(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Required(value, max);
    private static T Found<T>(T? value) where T : class => value ?? throw new KeyNotFoundException();
    private static bool Effective(DateTime from, DateTime? to, DateTime now) => from <= now && (!to.HasValue || to > now);
    private async Task ScopeAsync(int user, int branch, CancellationToken ct)
    {
        if (!await access.CanAccessBranchAsync(user, branch, ct)) throw new UnauthorizedAccessException();
    }
    private static byte[] Version(string? value)
    {
        var bytes = new byte[8];
        if (value is null || !Convert.TryFromBase64String(value, bytes, out int count) || count != 8)
            throw new ArgumentException("Phiên bản dữ liệu không hợp lệ.");
        return bytes;
    }
    private static decimal Amount(decimal value, int scale)
    {
        var limit = scale == 3 ? 1_000_000_000_000_000m : 10_000_000_000_000_000m;
        if (value < 0 || value >= limit || decimal.Round(value, scale) != value)
            throw new ArgumentException("Giá trị số vượt giới hạn hoặc có quá nhiều chữ số thập phân.");
        return value;
    }
    private static (DateTime From, DateTime To) Dates(DateOnly from, DateOnly to)
    {
        if (from.Year < 2000 || to.Year > 2100 || to.DayNumber - from.DayNumber is < 0 or > 90)
            throw new ArgumentException("Chọn khoảng ngày hợp lệ, tối đa 91 ngày.");
        return (from.ToDateTime(TimeOnly.MinValue).AddHours(-7), to.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7));
    }
    private async Task<int[]> BranchIdsAsync(int user, int? branch, CancellationToken ct)
    {
        var ids = (await access.GetAccessibleBranchesAsync(user, ct)).Select(b => b.Id).ToArray();
        if (branch.HasValue && !ids.Contains(branch.Value)) throw new UnauthorizedAccessException();
        return branch.HasValue ? [branch.Value] : ids;
    }

    public async Task<List<BranchView>> BranchesAsync(int user, bool all, CancellationToken ct)
    {
        var ids = all ? null : await BranchIdsAsync(user, null, ct);
        var now = Now;
        var assignments = await repo.AdminAssignmentsAsync(ct);
        return (await repo.BranchesAsync(ct)).Where(b => ids is null || ids.Contains(b.Id)).Select(b =>
        {
            var assignment = assignments.FirstOrDefault(a => a.BranchId == b.Id && Effective(a.EffectiveFrom, a.EffectiveTo, now));
            return new BranchView { Id = b.Id, Code = b.Code, Name = b.Name, Address = b.Address, IsActive = b.IsActive,
                AdminUserId = assignment?.AdminUserId, AdminName = assignment?.AdminUser.FullName ?? "" };
        }).ToList();
    }
    public async Task<int> SaveBranchAsync(int id, BranchView input, CancellationToken ct)
    {
        var branch = id == 0 ? new Branch { Code = Required(input.Code, 30).ToUpperInvariant() } : Found(await repo.BranchAsync(id, ct));
        branch.Name = Required(input.Name, 150);
        branch.Address = Required(input.Address, 500);
        branch.IsActive = input.IsActive;
        branch.UpdatedAt = Now;
        if (id == 0) repo.Add(branch);
        await work.SaveChangesAsync(ct);
        return branch.Id;
    }
    public async Task<List<UserView>> UsersAsync(int user, bool all, CancellationToken ct)
    {
        var ids = all ? null : await BranchIdsAsync(user, null, ct);
        var now = Now;
        var assignments = await repo.EmployeeAssignmentsAsync(ct);
        var permissions = await repo.StockPermissionsAsync(ct);
        var results = new List<UserView>();
        foreach (var account in await repo.UsersAsync(ct))
        {
            var profile = account.EmployeeProfile;
            var assignment = profile is { Deleted: false } ? assignments.FirstOrDefault(a => a.EmployeeProfileId == profile.Id && Effective(a.EffectiveFrom, a.EffectiveTo, now)) : null;
            if (ids is not null && (account.Role.Code != "EMPLOYEE" || assignment is null || !ids.Contains(assignment.BranchId))) continue;
            var permission = assignment is null ? null : permissions.FirstOrDefault(p => p.EmployeeProfileId == assignment.EmployeeProfileId && p.BranchId == assignment.BranchId);
            results.Add(new UserView { Id = account.Id, Username = account.Username, Role = account.Role.Code, FullName = account.FullName,
                Email = account.Email, Phone = account.Phone, IsActive = account.IsActive, Position = profile?.Position,
                EmployeeCode = profile?.EmployeeCode ?? "", BranchId = assignment?.BranchId, BranchName = assignment?.Branch.Name ?? "",
                CanReceive = permission?.CanReceive ?? false, CanIssue = permission?.CanIssue ?? false });
        }
        return results;
    }
    public async Task<int> CreateUserAsync(int actor, CreateUserRequest input, CancellationToken ct)
    {
        if (input.Role is not ("BRANCH_ADMIN" or "EMPLOYEE")) throw new ArgumentException("Chọn vai trò Admin Chi nhánh hoặc Nhân viên.");
        if (input.Password is not { Length: >= 12 and <= 128 } || string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Mật khẩu phải có 12–128 ký tự.");
        var role = Found(await repo.RoleAsync(input.Role, ct));
        var username = Required(input.Username, 100).ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-z0-9._-]+$"))
            throw new ArgumentException("Tên đăng nhập chỉ dùng chữ không dấu, số, dấu chấm, gạch dưới hoặc gạch ngang.");
        var account = new AppUser { RoleId = role.Id, Username = username, FullName = Required(input.FullName, 150),
            Email = Optional(input.Email, 254), Phone = Optional(input.Phone, 30), IsActive = true };
        account.PasswordHash = hasher.HashPassword(account, input.Password);
        if (input.Role == "EMPLOYEE")
            account.EmployeeProfile = new EmployeeProfile { EmployeeCode = "NV-" + Guid.NewGuid().ToString("N")[..20],
                HireDate = DateOnly.FromDateTime(Now.AddHours(7)) };
        // Một SaveChanges lưu cả tài khoản, hồ sơ và phân công ban đầu.
        if (input.BranchId is int branch)
        {
            await EnsureAssignableAsync(account, branch, ct, input.Role);
            AddAssignment(actor, account, branch, input.Role);
        }
        repo.Add(account);
        await work.SaveChangesAsync(ct);
        return account.Id;
    }
    public async Task<int> UpdateUserAsync(int actor, bool system, int id, UserView input, CancellationToken ct)
    {
        var account = Found(await repo.UserAsync(id, ct));
        if (account.Role.Code is not ("EMPLOYEE" or "BRANCH_ADMIN")) throw new UnauthorizedAccessException();
        var profile = account.EmployeeProfile;
        var now = Now;
        var assignment = profile is null ? null : (await repo.EmployeeAssignmentsAsync(ct))
            .FirstOrDefault(a => a.EmployeeProfileId == profile.Id && Effective(a.EffectiveFrom, a.EffectiveTo, now));
        if (!system)
        {
            if (account.Role.Code != "EMPLOYEE" || assignment is null || profile!.Deleted) throw new UnauthorizedAccessException();
            await ScopeAsync(actor, assignment.BranchId, ct);
        }
        account.FullName = Required(input.FullName, 150);
        account.Email = Optional(input.Email, 254);
        account.Phone = Optional(input.Phone, 30);
        account.UpdatedAt = now;
        if (profile is not null) profile.Position = Optional(input.Position, 100);
        if (system)
        {
            account.IsActive = input.IsActive;
            if (assignment is not null)
            {
                var permission = (await repo.StockPermissionsAsync(ct)).FirstOrDefault(p => p.BranchId == assignment.BranchId && p.EmployeeProfileId == assignment.EmployeeProfileId);
                if (permission is null)
                {
                    permission = new EmployeeStockPermission { EmployeeProfileId = assignment.EmployeeProfileId, BranchId = assignment.BranchId };
                    repo.Add(permission);
                }
                permission.CanReceive = input.CanReceive;
                permission.CanIssue = input.CanIssue;
            }
        }
        await work.SaveChangesAsync(ct);
        return id;
    }
    private async Task EnsureAssignableAsync(AppUser account, int branch, CancellationToken ct, string role)
    {
        if (!Found(await repo.BranchAsync(branch, ct)).IsActive || !account.IsActive)
            throw new ArgumentException("Tài khoản và chi nhánh phải đang hoạt động.");
        var now = Now;
        if (role == "BRANCH_ADMIN")
        {
            if ((await repo.AdminAssignmentsAsync(ct)).Any(a => a.BranchId == branch && (a.EffectiveTo == null || a.EffectiveTo > now)))
                throw new InvalidOperationException("Chi nhánh đã có quản lý hoặc phân công sắp có hiệu lực.");
        }
        else if (account.EmployeeProfile is not { Deleted: false } profile ||
                 (profile.Id != 0 && (await repo.EmployeeAssignmentsAsync(ct)).Any(a => a.EmployeeProfileId == profile.Id && (a.EffectiveTo == null || a.EffectiveTo > now))))
            throw new InvalidOperationException("Nhân viên đã có phân công hoặc hồ sơ không hợp lệ.");
    }
    private void AddAssignment(int actor, AppUser account, int branch, string role)
    {
        if (role == "BRANCH_ADMIN")
        {
            var assignment = new AdminBranchAssignment { BranchId = branch, AdminUser = account, AssignedByUserId = actor, EffectiveFrom = Now };
            if (account.Id == 0) account.AdminBranchAssignmentAdminUsers.Add(assignment);
            else repo.Add(assignment);
        }
        else
        {
            var profile = account.EmployeeProfile!;
            var assignment = new EmployeeBranchAssignment { BranchId = branch, EmployeeProfile = profile, AssignedByUserId = actor, EffectiveFrom = Now };
            if (profile.Id == 0) profile.EmployeeBranchAssignment = assignment;
            else repo.Add(assignment);
        }
    }
    public async Task<int> AssignAsync(int actor, AssignmentRequest input, CancellationToken ct)
    {
        var user = Found(await repo.UserAsync(input.UserId, ct));
        if (user.Role.Code is not ("EMPLOYEE" or "BRANCH_ADMIN")) throw new ArgumentException("Tài khoản không thể phân công.");
        await EnsureAssignableAsync(user, input.BranchId, ct, user.Role.Code);
        AddAssignment(actor, user, input.BranchId, user.Role.Code);
        await work.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<List<NamedOption>> OptionsAsync(string kind, CancellationToken ct) => kind switch
    {
        "categories" => (await repo.CategoriesAsync(ct)).Select(x => Option(x.Id, x.Name, x.IsActive)).ToList(),
        "sizes" => (await repo.SizesAsync(ct)).Select(x => Option(x.Id, x.Name, x.IsActive)).ToList(),
        "topping-groups" => (await repo.ToppingGroupsAsync(ct)).Select(x => Option(x.Id, x.Name, x.IsActive)).ToList(),
        "note-groups" => (await repo.NoteGroupsAsync(ct)).Select(x => Option(x.Id, x.Name, x.IsActive)).ToList(),
        "units" => (await repo.UnitsAsync(ct)).Select(x => Option(x.Id, x.Name, x.IsActive)).ToList(),
        _ => throw new KeyNotFoundException()
    };
    private static NamedOption Option(int id, string name, bool active) => new() { Id = id, Name = name, IsActive = active };
    public async Task<int> SaveOptionAsync(string kind, int id, NamedOption input, CancellationToken ct)
    {
        switch (kind)
        {
            case "categories":
                var category = id == 0 ? new Category() : Found((await repo.CategoriesAsync(ct)).SingleOrDefault(x => x.Id == id));
                category.Name = Required(input.Name, 150);
                category.IsActive = input.IsActive;
                if (id == 0) repo.Add(category);
                await work.SaveChangesAsync(ct);
                return category.Id;
            case "sizes":
                var size = id == 0 ? new Size() : Found((await repo.SizesAsync(ct)).SingleOrDefault(x => x.Id == id));
                size.Name = Required(input.Name, 50);
                size.IsActive = input.IsActive;
                if (id == 0) repo.Add(size);
                await work.SaveChangesAsync(ct);
                return size.Id;
            case "topping-groups":
                var group = id == 0 ? new ToppingGroup() : Found((await repo.ToppingGroupsAsync(ct)).SingleOrDefault(x => x.Id == id));
                group.Name = Required(input.Name, 100);
                group.IsActive = input.IsActive;
                if (id == 0) repo.Add(group);
                await work.SaveChangesAsync(ct);
                return group.Id;
            case "note-groups":
                var note = id == 0 ? new NoteGroup() : Found((await repo.NoteGroupsAsync(ct)).SingleOrDefault(x => x.Id == id));
                note.Name = Required(input.Name, 100);
                note.IsActive = input.IsActive;
                if (id == 0) repo.Add(note);
                await work.SaveChangesAsync(ct);
                return note.Id;
            default: throw new KeyNotFoundException();
        }
    }
    public async Task<List<ProductView>> ProductsAsync(int actor, int? branch, CancellationToken ct)
    {
        Dictionary<int, bool>? availability = null;
        if (branch is int id)
        {
            await ScopeAsync(actor, id, ct);
            availability = (await repo.BranchProductsAsync(id, ct)).ToDictionary(x => x.ProductId, x => x.IsAvailable);
        }
        return (await repo.ProductsAsync(ct)).Select(x => new ProductView { Id = x.Id, CategoryId = x.CategoryId,
            CategoryName = x.Category.Name, Name = x.Name, ImageUrl = x.ImageUrl, IsActive = x.IsActive,
            IsAvailable = availability?.GetValueOrDefault(x.Id) ?? true,
            Prices = x.ProductSizes.Select(p => new PriceView { Id = p.Id, SizeId = p.SizeId, SizeName = p.Size.Name, Price = p.Price, IsActive = p.IsActive }).ToList() }).ToList();
    }
    public async Task<int> SaveProductAsync(int id, ProductView input, CancellationToken ct)
    {
        var product = id == 0 ? new Product() : Found((await repo.ProductsAsync(ct)).SingleOrDefault(x => x.Id == id));
        if (!(await repo.CategoriesAsync(ct)).Any(x => x.Id == input.CategoryId)) throw new ArgumentException("Chọn danh mục hợp lệ.");
        product.CategoryId = input.CategoryId;
        product.Name = Required(input.Name, 150);
        var image = Optional(input.ImageUrl, 1000);
        if (image is not null && !(image.StartsWith("/images/", StringComparison.Ordinal) ||
            Uri.TryCreate(image, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
            throw new ArgumentException("Ảnh cần dùng URL HTTPS hoặc đường dẫn /images/.");
        product.ImageUrl = image;
        product.IsActive = input.IsActive;
        product.UpdatedAt = Now;
        if (input.Prices is null || input.Prices.Count > 30 || input.Prices.Any(x => x is null) ||
            input.Prices.Select(x => x.SizeId).Distinct().Count() != input.Prices.Count)
            throw new ArgumentException("Danh sách size không hợp lệ.");
        var sizes = await repo.SizesAsync(ct);
        foreach (var p in input.Prices)
        {
            if (!sizes.Any(x => x.Id == p.SizeId)) throw new ArgumentException("Size không tồn tại.");
            var row = product.ProductSizes.SingleOrDefault(x => x.SizeId == p.SizeId);
            if (row is null)
            {
                row = new ProductSize { SizeId = p.SizeId };
                product.ProductSizes.Add(row);
            }
            row.Price = Amount(p.Price, 2);
            row.IsActive = p.IsActive;
        }
        if (id == 0) repo.Add(product);
        await work.SaveChangesAsync(ct);
        return product.Id;
    }
    public async Task<List<ToppingView>> ToppingsAsync(int actor, int? branch, CancellationToken ct)
    {
        Dictionary<int, bool>? availability = null;
        if (branch is int id)
        {
            await ScopeAsync(actor, id, ct);
            availability = (await repo.BranchToppingsAsync(id, ct)).ToDictionary(x => x.ToppingId, x => x.IsAvailable);
        }
        return (await repo.ToppingsAsync(ct)).Select(x => new ToppingView { Id = x.Id, GroupId = x.ToppingGroupId,
            GroupName = x.ToppingGroup.Name, Name = x.Name, Price = x.Price, IsActive = x.IsActive, IsAvailable = availability?.GetValueOrDefault(x.Id) ?? true }).ToList();
    }
    public async Task<int> SaveToppingAsync(int id, ToppingView input, CancellationToken ct)
    {
        var topping = id == 0 ? new Topping() : Found((await repo.ToppingsAsync(ct)).SingleOrDefault(x => x.Id == id));
        if (!(await repo.ToppingGroupsAsync(ct)).Any(x => x.Id == input.GroupId)) throw new ArgumentException("Chọn nhóm topping hợp lệ.");
        topping.ToppingGroupId = input.GroupId;
        topping.Name = Required(input.Name, 150);
        topping.Price = Amount(input.Price, 2);
        topping.IsActive = input.IsActive;
        if (id == 0) repo.Add(topping);
        await work.SaveChangesAsync(ct);
        return topping.Id;
    }
    public async Task<List<NoteView>> NotesAsync(CancellationToken ct) => (await repo.NoteGroupsAsync(ct))
        .SelectMany(g => g.NoteOptions.Where(n => !n.Deleted).Select(n => new NoteView { Id = n.Id, GroupId = g.Id, GroupName = g.Name, Name = n.Name, IsActive = n.IsActive })).ToList();
    public async Task<int> SaveNoteAsync(int id, NoteView input, CancellationToken ct)
    {
        var groups = await repo.NoteGroupsAsync(ct);
        var note = id == 0 ? new NoteOption() : Found(groups.SelectMany(g => g.NoteOptions).SingleOrDefault(n => n.Id == id && !n.Deleted));
        if (!groups.Any(g => g.Id == input.GroupId)) throw new ArgumentException("Chọn nhóm ghi chú hợp lệ.");
        note.NoteGroupId = input.GroupId;
        note.Name = Required(input.Name, 100);
        note.IsActive = input.IsActive;
        if (id == 0) repo.Add(note);
        await work.SaveChangesAsync(ct);
        return note.Id;
    }
    public async Task<int> SetAvailabilityAsync(int actor, int branch, string kind, int id, bool enabled, CancellationToken ct)
    {
        await ScopeAsync(actor, branch, ct);
        if (kind == "products")
        {
            _ = Found((await repo.ProductsAsync(ct)).SingleOrDefault(x => x.Id == id));
            var row = (await repo.BranchProductsAsync(branch, ct)).SingleOrDefault(x => x.ProductId == id);
            if (row is null) repo.Add(new BranchProduct { BranchId = branch, ProductId = id, IsAvailable = enabled, UpdatedAt = Now });
            else { row.IsAvailable = enabled; row.UpdatedAt = Now; }
        }
        else if (kind == "toppings")
        {
            _ = Found((await repo.ToppingsAsync(ct)).SingleOrDefault(x => x.Id == id));
            var row = (await repo.BranchToppingsAsync(branch, ct)).SingleOrDefault(x => x.ToppingId == id);
            if (row is null) repo.Add(new BranchTopping { BranchId = branch, ToppingId = id, IsAvailable = enabled, UpdatedAt = Now });
            else { row.IsAvailable = enabled; row.UpdatedAt = Now; }
        }
        else throw new KeyNotFoundException();
        await work.SaveChangesAsync(ct);
        return id;
    }

    public async Task<List<IngredientView>> IngredientsAsync(CancellationToken ct) => (await repo.IngredientsAsync(ct))
        .Select(x => new IngredientView { Id = x.Id, Name = x.Name, UnitId = x.UnitId, UnitName = x.Unit.Name, IsActive = x.IsActive }).ToList();
    public async Task<int> SaveIngredientAsync(int id, IngredientView input, CancellationToken ct)
    {
        var row = id == 0 ? new Ingredient() : Found((await repo.IngredientsAsync(ct)).SingleOrDefault(x => x.Id == id));
        if (!(await repo.UnitsAsync(ct)).Any(x => x.Id == input.UnitId && x.IsActive)) throw new ArgumentException("Chọn đơn vị hợp lệ.");
        if (id != 0 && row.UnitId != input.UnitId) throw new ArgumentException("Đơn vị của nguyên liệu đã tạo được giữ để bảo toàn số liệu tồn.");
        row.Name = Required(input.Name, 150);
        row.UnitId = input.UnitId;
        row.IsActive = input.IsActive;
        if (id == 0) repo.Add(row);
        await work.SaveChangesAsync(ct);
        return row.Id;
    }
    public async Task<List<BalanceView>> BalancesAsync(int actor, int branch, CancellationToken ct)
    {
        await ScopeAsync(actor, branch, ct);
        var balances = (await repo.BalancesAsync(branch, ct)).ToDictionary(x => x.IngredientId);
        return (await repo.IngredientsAsync(ct)).Where(x => x.IsActive || balances.ContainsKey(x.Id)).Select(x =>
        {
            var balance = balances.GetValueOrDefault(x.Id);
            return new BalanceView { IngredientId = x.Id, Name = x.Name, Unit = x.Unit.Name, Quantity = balance?.Quantity ?? 0,
                WarningThreshold = balance?.WarningThreshold ?? 0, RowVersion = balance is null ? "" : Convert.ToBase64String(balance.RowVersion) };
        }).ToList();
    }
    public async Task<int> ThresholdAsync(int actor, int branch, int ingredient, ThresholdRequest input, CancellationToken ct)
    {
        await ScopeAsync(actor, branch, ct);
        var row = Found((await repo.BalancesAsync(branch, ct)).SingleOrDefault(x => x.IngredientId == ingredient));
        repo.ExpectVersion(row, Version(input.RowVersion));
        row.WarningThreshold = Amount(input.WarningThreshold, 3);
        row.UpdatedAt = Now;
        await work.SaveChangesAsync(ct);
        return row.Id;
    }
    public async Task<List<StockHistoryView>> StockHistoryAsync(int actor, int branch, CancellationToken ct)
    {
        await ScopeAsync(actor, branch, ct);
        return (await repo.StockHistoryAsync(branch, ct)).Select(x => new StockHistoryView { Id = x.Id, Type = x.Type,
            CreatedAt = x.CreatedAt, PerformedBy = x.PerformedByUser.FullName,
            Lines = x.StockTransactionDetails.Select(d => new StockHistoryLine { Name = d.Ingredient.Name, Change = d.QuantityChange, Before = d.QuantityBefore, After = d.QuantityAfter }).ToList() }).ToList();
    }
    public async Task<int> WriteStockAsync(int actor, bool employee, StockWriteRequest input, CancellationToken ct)
    {
        await ScopeAsync(actor, input.BranchId, ct);
        if (input.Type is not ("IN" or "OUT") || input.Lines is not { Count: >= 1 and <= 50 } ||
            input.Lines.Any(x => x is null) || input.Lines.Select(x => x.IngredientId).Distinct().Count() != input.Lines.Count)
            throw new ArgumentException("Chọn nhập/xuất và 1–50 nguyên liệu khác nhau.");
        var reason = Required(input.Reason, 500);
        if (employee)
        {
            var profile = (await repo.UserAsync(actor, ct))?.EmployeeProfile;
            if (profile is null || profile.Deleted || !(await repo.StockPermissionsAsync(ct)).Any(x => x.EmployeeProfileId == profile.Id &&
                x.BranchId == input.BranchId && (input.Type == "IN" ? x.CanReceive : x.CanIssue)))
                throw new UnauthorizedAccessException();
        }
        var ingredients = (await repo.IngredientsAsync(ct)).Where(x => x.IsActive).Select(x => x.Id).ToHashSet();
        var balances = (await repo.BalancesAsync(input.BranchId, ct)).ToDictionary(x => x.IngredientId);
        var now = Now;
        var tx = new StockTransaction { BranchId = input.BranchId, Type = input.Type, PerformedByUserId = actor, CreatedAt = now };
        foreach (var inputLine in input.Lines.OrderBy(x => x.IngredientId))
        {
            var quantity = Amount(inputLine.Quantity, 3);
            if (quantity == 0 || !ingredients.Contains(inputLine.IngredientId)) throw new ArgumentException("Nguyên liệu hoặc số lượng không hợp lệ.");
            var row = balances.GetValueOrDefault(inputLine.IngredientId);
            if (row is null)
            {
                if (!string.IsNullOrEmpty(inputLine.RowVersion)) throw new InvalidOperationException("Tồn kho đã thay đổi.");
                row = new BranchIngredient { BranchId = input.BranchId, IngredientId = inputLine.IngredientId };
                repo.Add(row);
            }
            else repo.ExpectVersion(row, Version(inputLine.RowVersion));
            var before = row.Quantity;
            var change = input.Type == "IN" ? quantity : -quantity;
            if (before + change < 0) throw new ArgumentException("Số lượng xuất vượt tồn hiện tại.");
            row.Quantity = Amount(before + change, 3);
            row.UpdatedAt = now;
            tx.StockTransactionDetails.Add(new StockTransactionDetail { IngredientId = row.IngredientId,
                QuantityBefore = before, QuantityChange = change, QuantityAfter = row.Quantity });
        }
        // Ghi tồn, phiếu và lý do trong cùng giao dịch.
        await work.BeginTransactionAsync(ct);
        try
        {
            repo.Add(tx);
            await work.SaveChangesAsync(ct);
            repo.Add(new AuditLog { BranchId = input.BranchId, EntityName = "StockTransaction", EntityId = tx.Id, Action = "CREATE",
                PerformedByUserId = actor, CreatedAt = now, AfterData = JsonSerializer.Serialize(new { Reason = reason }) });
            await work.SaveChangesAsync(ct);
        }
        catch { await work.RollbackTransactionAsync(CancellationToken.None); throw; }
        await work.CommitTransactionAsync(ct);
        return tx.Id;
    }

    private static ManagedOrderView Order(SalesOrder order) => new() { Id = order.Id, BranchId = order.BranchId, Code = order.Code,
        BranchName = order.Branch.Name, EmployeeName = order.CreatedByUser.FullName, Status = order.Status, TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt, RowVersion = Convert.ToBase64String(order.RowVersion) };
    public async Task<List<ManagedOrderView>> OrdersAsync(int actor, int? branch, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var range = Dates(from, to);
        return (await repo.OrdersAsync(await BranchIdsAsync(actor, branch, ct), range.From, range.To, ct)).Select(Order).ToList();
    }
    public async Task<ManagedOrderView> OrderAsync(int actor, int id, CancellationToken ct)
    {
        var order = Found(await repo.OrderAsync(id, ct));
        await ScopeAsync(actor, order.BranchId, ct);
        var view = Order(order);
        view.Items = order.OrderItems.OrderBy(x => x.Id).Select(x => new ManagedOrderLine { Id = x.Id, Name = x.ProductNameSnapshot,
            Size = x.SizeNameSnapshot, Quantity = x.Quantity, UnitAmount = UnitAmount(x), Subtotal = x.SubtotalAmount,
            Options = string.Join(" · ", x.OrderItemToppings.Select(t => $"{t.ToppingNameSnapshot} × {t.Quantity}").Concat(x.OrderItemNotes.Select(n => n.NoteNameSnapshot))) }).ToList();
        view.History = order.OrderAdjustments.OrderByDescending(x => x.CreatedAt).Select(x => new AdjustmentView { CreatedAt = x.CreatedAt,
            PerformedBy = x.AdjustedByUser.FullName, Reason = x.Reason, BeforeData = x.BeforeData, AfterData = x.AfterData }).ToList();
        return view;
    }
    private static decimal UnitAmount(OrderItem item) => item.UnitPriceSnapshot + item.OrderItemToppings.Sum(t => t.UnitPriceSnapshot * t.Quantity);
    private static string Snapshot(SalesOrder order) => JsonSerializer.Serialize(new { order.Status, order.TotalAmount,
        Items = order.OrderItems.OrderBy(x => x.Id).Select(x => new { x.Id, x.ProductNameSnapshot, x.SizeNameSnapshot, x.Quantity, x.SubtotalAmount }) });

    public async Task<int> AdjustOrderAsync(int actor, int id, AdjustOrderRequest input, CancellationToken ct)
    {
        var order = Found(await repo.OrderAsync(id, ct));
        await ScopeAsync(actor, order.BranchId, ct);
        if (order.Status == "CANCELLED") throw new InvalidOperationException("Đơn đã hủy không thể chỉnh sửa.");
        var reason = Required(input.Reason, 500);
        repo.ExpectVersion(order, Version(input.RowVersion));
        var before = Snapshot(order);
        if (input.Cancel) order.Status = "CANCELLED";
        else
        {
            if (input.Items is not { Count: >= 1 and <= 100 } || input.Items.Any(x => x is null || x.Quantity is < 1 or > 999) ||
                input.Items.Select(x => x.ItemId).Distinct().Count() != input.Items.Count)
                throw new ArgumentException("Số ly phải từ 1–999 và mỗi dòng chỉ được sửa một lần.");
            foreach (var change in input.Items)
            {
                var line = order.OrderItems.SingleOrDefault(x => x.Id == change.ItemId) ?? throw new ArgumentException("Dòng món không thuộc đơn.");
                line.Quantity = change.Quantity;
                line.SubtotalAmount = UnitAmount(line) * change.Quantity;
            }
            order.TotalAmount = Amount(order.OrderItems.Sum(x => x.SubtotalAmount), 2);
        }
        order.UpdatedAt = Now;
        repo.Add(new OrderAdjustment { SalesOrderId = id, AdjustedByUserId = actor, Reason = reason, BeforeData = before, AfterData = Snapshot(order), CreatedAt = Now });
        await work.SaveChangesAsync(ct);
        return id;
    }
    public async Task<List<RevenueView>> RevenueAsync(int actor, int? branch, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var range = Dates(from, to);
        return (await repo.RevenueAsync(await BranchIdsAsync(actor, branch, ct), range.From, range.To, ct))
            .Select(x => new RevenueView { BranchId = x.BranchId, BranchName = x.BranchName,
                Date = DateOnly.FromDateTime(x.VietnamDate), Orders = x.Orders, Amount = x.Amount }).ToList();
    }
}
