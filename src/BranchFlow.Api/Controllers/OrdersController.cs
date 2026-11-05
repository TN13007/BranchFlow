using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "EMPLOYEE")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    // Chỉ nhận ID và số lượng; giá lạ trong JSON bị từ chối bởi DTO contract.
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var result = await orders.CreateAsync(UserId, request, ct);
            return Created($"/api/orders/{result.Id}", result);
        }
        catch (OrderValidationException ex) { return Problem(statusCode: 400, title: "Dữ liệu không hợp lệ", detail: ex.Message); }
        catch (OverflowException) { return Problem(statusCode: 400, title: "Giá trị vượt giới hạn"); }
        catch (UnauthorizedAccessException) { return Problem(statusCode: 403, title: "Không có quyền tại chi nhánh"); }
        catch (OrderBranchUnavailableException) { return Problem(statusCode: 404, title: "Chi nhánh không hoạt động"); }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> MineAsync([FromQuery] string? keyword, [FromQuery] DateOnly? businessDate, CancellationToken ct)
    {
        try { return Ok(await orders.GetMineAsync(UserId, keyword, businessDate, ct)); }
        catch (OrderValidationException ex) { return Problem(statusCode: 400, title: "Bộ lọc không hợp lệ", detail: ex.Message); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> DetailAsync(int id, CancellationToken ct)
    {
        var result = await orders.GetDetailAsync(UserId, id, ct);
        return result is null ? Problem(statusCode: 404, title: "Không tìm thấy đơn trong phạm vi của bạn") : Ok(result);
    }
}
