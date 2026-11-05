using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public sealed class BranchesController : ControllerBase
{
    private readonly IBranchAccessService _branchAccessService;

    public BranchesController(IBranchAccessService branchAccessService)
    {
        _branchAccessService = branchAccessService;
    }

    [HttpGet("accessible")]
    public async Task<IActionResult> GetAccessibleBranches(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId) || userId <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Chưa xác thực",
                detail: "Vui lòng đăng nhập lại.");
        }

        IReadOnlyList<BranchFlow.Application.DTOs.Sprint4.AccessibleBranch> branches =
            await _branchAccessService.GetAccessibleBranchesAsync(userId, cancellationToken);

        return Ok(branches);
    }
}
