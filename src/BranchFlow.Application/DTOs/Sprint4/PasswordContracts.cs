namespace BranchFlow.Application.DTOs.Sprint4;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record ForgotPasswordRequest(string Username);
public sealed record AdminResetPasswordRequest(bool IdentityVerified, string Reason);
public sealed record TemporaryPasswordResult(string Username, string TemporaryPassword, DateTimeOffset ExpiresAt);
public sealed record PasswordSupportRequest(int Id, int UserId, string Username, string FullName, DateTimeOffset RequestedAt);
