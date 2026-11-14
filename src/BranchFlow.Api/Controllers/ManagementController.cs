using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/management")]
public sealed class ManagementController(MvpService service) : ControllerBase
{
    private const string System = "SYSTEM_ADMIN";
    private const string Admins = "SYSTEM_ADMIN,BRANCH_ADMIN";
    private const string Readers = "OWNER,SYSTEM_ADMIN,BRANCH_ADMIN";
    private const string StockWriters = "SYSTEM_ADMIN,BRANCH_ADMIN,EMPLOYEE";
    private bool SystemAdmin => User.IsInRole(System);
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("branches")]
    public Task<IActionResult> Branches(CancellationToken ct) =>
        RunAsync(() => service.BranchesAsync(UserId, SystemAdmin, ct));

    [HttpPost("branches")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateBranch(BranchView input, CancellationToken ct) =>
        RunAsync(() => service.SaveBranchAsync(0, input, ct));

    [HttpPut("branches/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateBranch(int id, BranchView input, CancellationToken ct) =>
        RunAsync(() => service.SaveBranchAsync(id, input, ct));

    [HttpGet("employee-directory")]
    [Authorize(Roles = "OWNER")]
    public Task<IActionResult> EmployeeDirectory(int? branchId, CancellationToken ct) =>
        RunAsync(() => service.EmployeeDirectoryAsync(UserId, branchId, ct));

    [HttpGet("users")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> Users(CancellationToken ct) =>
        RunAsync(() => service.UsersAsync(UserId, SystemAdmin, ct));

    [HttpPost("users")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateUser(CreateUserRequest input, CancellationToken ct) =>
        RunAsync(() => service.CreateUserAsync(UserId, input, ct));

    [HttpPut("users/{id:int}")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> UpdateUser(int id, UserView input, CancellationToken ct) =>
        RunAsync(() => service.UpdateUserAsync(UserId, SystemAdmin, id, input, ct));

    [HttpPost("assignments")]
    [Authorize(Roles = System)]
    public Task<IActionResult> Assign(AssignmentRequest input, CancellationToken ct) =>
        RunAsync(() => service.AssignAsync(UserId, input, ct));

    [HttpGet("options/{kind}")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> Options(string kind, CancellationToken ct) =>
        RunAsync(() => service.OptionsAsync(kind, ct));

    [HttpPost("options/{kind}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateOption(string kind, NamedOption input, CancellationToken ct) =>
        RunAsync(() => service.SaveOptionAsync(kind, 0, input, ct));

    [HttpPut("options/{kind}/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateOption(string kind, int id, NamedOption input, CancellationToken ct) =>
        RunAsync(() => service.SaveOptionAsync(kind, id, input, ct));

    [HttpGet("products")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> Products(int? branchId, CancellationToken ct) =>
        RunAsync(() => service.ProductsAsync(UserId, branchId, ct));

    [HttpPost("products")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateProduct(ProductView input, CancellationToken ct) =>
        RunAsync(() => service.SaveProductAsync(0, input, ct));

    [HttpPut("products/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateProduct(int id, ProductView input, CancellationToken ct) =>
        RunAsync(() => service.SaveProductAsync(id, input, ct));

    [HttpGet("toppings")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> Toppings(int? branchId, CancellationToken ct) =>
        RunAsync(() => service.ToppingsAsync(UserId, branchId, ct));

    [HttpPost("toppings")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateTopping(ToppingView input, CancellationToken ct) =>
        RunAsync(() => service.SaveToppingAsync(0, input, ct));

    [HttpPut("toppings/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateTopping(int id, ToppingView input, CancellationToken ct) =>
        RunAsync(() => service.SaveToppingAsync(id, input, ct));

    [HttpGet("notes")]
    [Authorize(Roles = System)]
    public Task<IActionResult> Notes(CancellationToken ct) =>
        RunAsync(() => service.NotesAsync(ct));

    [HttpPost("notes")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateNote(NoteView input, CancellationToken ct) =>
        RunAsync(() => service.SaveNoteAsync(0, input, ct));

    [HttpPut("notes/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateNote(int id, NoteView input, CancellationToken ct) =>
        RunAsync(() => service.SaveNoteAsync(id, input, ct));

    [HttpPut("branches/{branchId:int}/availability/{kind}/{id:int}")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> SetAvailability(int branchId, string kind, int id, AvailabilityRequest input, CancellationToken ct) =>
        RunAsync(() => service.SetAvailabilityAsync(UserId, branchId, kind, id, input.IsAvailable, ct));

    [HttpGet("ingredients")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> Ingredients(CancellationToken ct) =>
        RunAsync(() => service.IngredientsAsync(ct));

    [HttpPost("ingredients")]
    [Authorize(Roles = System)]
    public Task<IActionResult> CreateIngredient(IngredientView input, CancellationToken ct) =>
        RunAsync(() => service.SaveIngredientAsync(0, input, ct));

    [HttpPut("ingredients/{id:int}")]
    [Authorize(Roles = System)]
    public Task<IActionResult> UpdateIngredient(int id, IngredientView input, CancellationToken ct) =>
        RunAsync(() => service.SaveIngredientAsync(id, input, ct));

    [HttpGet("branches/{branchId:int}/stock")]
    public Task<IActionResult> Balances(int branchId, CancellationToken ct) =>
        RunAsync(() => service.BalancesAsync(UserId, branchId, ct));

    [HttpGet("branches/{branchId:int}/stock/history")]
    public Task<IActionResult> StockHistory(int branchId, CancellationToken ct) =>
        RunAsync(() => service.StockHistoryAsync(UserId, branchId, ct));

    [HttpGet("branches/{branchId:int}/stock/capabilities")]
    [Authorize(Roles = Readers + ",EMPLOYEE")]
    public Task<IActionResult> StockAccess(int branchId, CancellationToken ct) =>
        RunAsync(() => service.StockCapabilitiesAsync(UserId, branchId, User.IsInRole("EMPLOYEE"),
            SystemAdmin || User.IsInRole("BRANCH_ADMIN"), ct));

    [HttpPut("branches/{branchId:int}/stock/{ingredientId:int}/threshold")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> UpdateThreshold(int branchId, int ingredientId, ThresholdRequest input, CancellationToken ct) =>
        RunAsync(() => service.ThresholdAsync(UserId, branchId, ingredientId, input, ct));

    [HttpPost("stock")]
    [Authorize(Roles = StockWriters)]
    public Task<IActionResult> WriteStock(StockWriteRequest input, CancellationToken ct) =>
        RunAsync(() => service.WriteStockAsync(UserId, User.IsInRole("EMPLOYEE"), input, ct));

    [HttpGet("orders")]
    [Authorize(Roles = Readers)]
    public Task<IActionResult> Orders(int? branchId, DateOnly from, DateOnly to, CancellationToken ct) =>
        RunAsync(() => service.OrdersAsync(UserId, branchId, from, to, ct));

    [HttpGet("orders/{id:int}")]
    [Authorize(Roles = Readers)]
    public Task<IActionResult> Order(int id, CancellationToken ct) =>
        RunAsync(() => service.OrderAsync(UserId, id, ct));

    [HttpPut("orders/{id:int}")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> AdjustOrder(int id, AdjustOrderRequest input, CancellationToken ct) =>
        RunAsync(() => service.AdjustOrderAsync(UserId, id, input, ct));
    [HttpPost("orders/{id:int}/cancel")]
    [Authorize(Roles = Admins)]
    public Task<IActionResult> CancelOrder(int id, CancelOrderRequest input, CancellationToken ct) =>
        RunAsync(() => service.CancelOrderAsync(UserId, id, input, ct));

    [HttpGet("revenue")]
    [Authorize(Roles = Readers)]
    public Task<IActionResult> Revenue(int? branchId, DateOnly from, DateOnly to, CancellationToken ct) =>
        RunAsync(() => service.RevenueAsync(UserId, branchId, from, to, ct));

    private async Task<IActionResult> RunAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException ex) { return Problem(statusCode: 400, detail: ex.Message); }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Chi nhánh đã", StringComparison.Ordinal) ||
            ex.Message.StartsWith("Nhân viên đã", StringComparison.Ordinal) || ex.Message.StartsWith("Đơn đã", StringComparison.Ordinal) ||
            ex.Message.StartsWith("Tồn kho đã", StringComparison.Ordinal))
        { return Problem(statusCode: 409, detail: ex.Message); }
        catch (DbUpdateConcurrencyException) { return Problem(statusCode: 409, detail: "Dữ liệu đã thay đổi. Hãy tải lại."); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { return Problem(statusCode: 409, detail: "Mã, tên hoặc phân công đã tồn tại."); }
    }
}
