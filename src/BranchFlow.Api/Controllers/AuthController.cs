using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BranchFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId) || userId <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Chưa xác thực",
                detail: "Vui lòng đăng nhập lại.");
        }

        BranchFlow.Application.DTOs.Sprint4.CurrentUser? currentUser =
            await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (currentUser is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Chưa xác thực",
                detail: "Vui lòng đăng nhập lại.");
        }

        return Ok(currentUser);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest? request,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrEmpty(request.Password))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu không hợp lệ",
                detail: "Username và password là bắt buộc.");
        }

        TokenPair? tokenPair = await _authService.LoginAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (tokenPair is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Đăng nhập không thành công",
                detail: "Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        return Ok(tokenPair);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest? request,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        if (request is null || string.IsNullOrEmpty(request.RefreshToken))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu không hợp lệ",
                detail: "Refresh token là bắt buộc.");
        }

        TokenPair? tokenPair = await _authService.RefreshAsync(
            request.RefreshToken,
            cancellationToken);

        if (tokenPair is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Phiên không hợp lệ",
                detail: "Refresh token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
        }

        return Ok(tokenPair);
    }
}
