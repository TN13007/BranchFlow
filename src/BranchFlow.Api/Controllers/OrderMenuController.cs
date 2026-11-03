using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api/order-menu")]
[Authorize(Roles = "EMPLOYEE")]
public sealed class OrderMenuController : ControllerBase
{
    private readonly IOrderMenuService _orderMenuService;

    public OrderMenuController(IOrderMenuService orderMenuService)
    {
        _orderMenuService = orderMenuService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuByBranch([FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue || branchId.Value <= 0)
        {
            return Problem(statusCode: 400, title: "Dữ liệu không hợp lệ", detail: "branchId phải là số nguyên dương.");
        }

        if (!int.TryParse(User.FindFirst("sub")?.Value, out var userId) || userId <= 0)
            return Problem(statusCode: 401, title: "Chưa xác thực", detail: "Vui lòng đăng nhập lại.");

        BranchFlow.Application.DTOs.Sprint4.OrderMenu? menu;
        try
        {
            menu = await _orderMenuService.GetMenuByBranchAsync(userId, branchId.Value, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Problem(statusCode: 403, title: "Không có quyền", detail: "Bạn không được phân công tại chi nhánh này.");
        }

        if (menu is null)
        {
            return Problem(statusCode: 404, title: "Không tìm thấy", detail: "Chi nhánh không tồn tại hoặc không hoạt động.");
        }

        return Ok(menu);
    }
}
