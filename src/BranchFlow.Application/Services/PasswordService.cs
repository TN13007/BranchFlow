using System.Security.Cryptography;
using System.Text.Json;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace BranchFlow.Application.Services;

public sealed class PasswordService(PasswordRepository repo, IUnitOfWork work, IPasswordHasher<AppUser> hasher, TimeProvider clock)
{
    public const string RequestAcknowledgement = "Nếu tài khoản hợp lệ, yêu cầu đã được ghi nhận. Vui lòng liên hệ Admin Tổng để xác minh danh tính và nhận mật khẩu tạm.";

    public async Task RequestAsync(string? username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length > 100) return;
        var user = await repo.UserAsync(username.Trim(), ct);
        if (user?.Role.Code is "EMPLOYEE" or "BRANCH_ADMIN")
            await repo.RequestAsync(user.Id, clock.GetUtcNow().UtcDateTime, ct);
        // No password change, session revocation, email or account information in this anonymous response.
    }

    public async Task<List<PasswordSupportRequest>> PendingAsync(int actor, CancellationToken ct = default)
    {
        await RequireAdminAsync(actor, ct);
        return (await repo.PendingAsync(ct)).Select(x => new PasswordSupportRequest(x.Id, x.UserId,
            x.User.Username, x.User.FullName, Utc(x.RequestedAt))).ToList();
    }

    public async Task ChangeAsync(int userId, Guid sessionId, ChangePasswordRequest input, CancellationToken ct = default)
    {
        var user = await repo.UserAsync(userId, ct) ?? throw new UnauthorizedAccessException();
        var now = clock.GetUtcNow().UtcDateTime;
        if (!await repo.ActiveSessionAsync(user, sessionId, now, ct) ||
            (user.MustChangePassword && (user.TemporaryPasswordExpiresAt is null || user.TemporaryPasswordExpiresAt <= now)))
            throw new UnauthorizedAccessException();
        if (string.IsNullOrEmpty(input.CurrentPassword) || input.CurrentPassword.Length > 128 ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, input.CurrentPassword) == PasswordVerificationResult.Failed)
            throw new ArgumentException("Mật khẩu hiện tại không đúng.");
        if (string.IsNullOrWhiteSpace(input.NewPassword) || input.NewPassword.Length is < 12 or > 128)
            throw new ArgumentException("Mật khẩu mới phải có 12–128 ký tự.");
        if (input.NewPassword != input.ConfirmPassword) throw new ArgumentException("Hai lần nhập mật khẩu mới chưa khớp.");
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, input.NewPassword) != PasswordVerificationResult.Failed)
            throw new ArgumentException("Mật khẩu mới phải khác mật khẩu hiện tại.");
        await ReplaceAsync(user, userId, input.NewPassword, null, "PASSWORD_CHANGED", null, ct);
    }

    public async Task<TemporaryPasswordResult> ResetAsync(int actor, int target, AdminResetPasswordRequest input, CancellationToken ct = default)
    {
        await RequireAdminAsync(actor, ct);
        if (!input.IdentityVerified || string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length is < 5 or > 300)
            throw new ArgumentException("Xác minh danh tính qua kênh liên hệ đã biết và nhập lý do từ 5 đến 300 ký tự.");
        var user = await repo.UserAsync(target, ct) ?? throw new KeyNotFoundException("Không tìm thấy tài khoản đang hoạt động.");
        if (user.Role.Code is not ("BRANCH_ADMIN" or "EMPLOYEE")) throw new UnauthorizedAccessException();
        string temporary = "T!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)) + "a9";
        var expires = clock.GetUtcNow().AddMinutes(30);
        await ReplaceAsync(user, actor, temporary, expires.UtcDateTime, "PASSWORD_RESET", input.Reason.Trim(), ct);
        return new(user.Username, temporary, expires);
    }

    private async Task RequireAdminAsync(int actor, CancellationToken ct)
    {
        if ((await repo.UserAsync(actor, ct))?.Role.Code != "SYSTEM_ADMIN") throw new UnauthorizedAccessException();
    }

    private async Task ReplaceAsync(AppUser user, int actor, string password, DateTime? expires, string action, string? reason, CancellationToken ct)
    {
        var hash = hasher.HashPassword(user, password);
        var now = clock.GetUtcNow().UtcDateTime;
        await work.BeginTransactionAsync(ct);
        try
        {
            if (await repo.ReplaceAsync(user, hash, expires.HasValue, expires, now, ct) != 1)
                throw new InvalidOperationException("Thông tin bảo mật vừa thay đổi. Vui lòng đăng nhập lại trước khi tiếp tục.");
            await repo.RevokeAndResolveAsync(user.Id, actor, now, ct);
            repo.Audit(new AuditLog { EntityName = "AppUser", EntityId = user.Id, Action = "UPDATE",
                PerformedByUserId = actor, CreatedAt = now,
                AfterData = JsonSerializer.Serialize(new { Event = action, user.Username, MustChangePassword = expires.HasValue,
                    TemporaryPasswordExpiresAt = expires, Reason = reason }) });
            await work.SaveChangesAsync(ct);
            await work.CommitTransactionAsync(ct);
        }
        catch { await work.RollbackTransactionAsync(CancellationToken.None); throw; }
    }

    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
