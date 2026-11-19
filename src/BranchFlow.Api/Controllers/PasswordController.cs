using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class PasswordController(PasswordService service) : ControllerBase
{
    private int Actor => int.Parse(User.FindFirst("sub")!.Value);

    [HttpPost("auth/forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("password-support")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequest input, CancellationToken ct)
    {
        await service.RequestAsync(input.Username, ct);
        return Ok(new { Message = PasswordService.RequestAcknowledgement });
    }

    [HttpPost("auth/change-password")]
    [Authorize]
    [EnableRateLimiting("password-support")]
    public Task<IActionResult> Change(ChangePasswordRequest input, CancellationToken ct) => Run(async () =>
    {
        await service.ChangeAsync(Actor, Guid.Parse(User.FindFirst("sid")!.Value), input, ct);
        return NoContent();
    });

    [HttpGet("management/password-requests")]
    [Authorize(Roles = "SYSTEM_ADMIN")]
    public Task<IActionResult> Pending(CancellationToken ct) => Run(async () => Ok(await service.PendingAsync(Actor, ct)));

    [HttpPost("management/users/{id:int}/reset-password")]
    [Authorize(Roles = "SYSTEM_ADMIN")]
    [EnableRateLimiting("password-support")]
    public Task<IActionResult> Reset(int id, AdminResetPasswordRequest input, CancellationToken ct) =>
        Run(async () => Ok(await service.ResetAsync(Actor, id, input, ct)));

    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (ArgumentException ex) { return Problem(statusCode: 400, title: "Dữ liệu chưa hợp lệ", detail: ex.Message); }
        catch (UnauthorizedAccessException) { return Problem(statusCode: 403, title: "Không có quyền", detail: "Tài khoản hoặc phiên không cho phép thao tác này."); }
        catch (KeyNotFoundException ex) { return Problem(statusCode: 404, title: "Không tìm thấy", detail: ex.Message); }
        catch (InvalidOperationException ex) { return Problem(statusCode: 409, title: "Xung đột", detail: ex.Message); }
    }
}
