using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api/order-menu")]
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
            return BadRequest(new { message = "Branch ID must be greater than zero." });
        }

        var menu = await _orderMenuService.GetMenuByBranchAsync(branchId.Value, cancellationToken);

        if (menu is null)
        {
            return NotFound(new { message = "Branch not found." });
        }

        return Ok(menu);
    }
}