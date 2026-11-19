namespace BranchFlow.Infrastructure.Repositories;

public sealed record AuthUserData(
    int Id,
    string Username,
    string PasswordHash,
    string FullName,
    string RoleCode,
    bool MustChangePassword = false,
    DateTime? TemporaryPasswordExpiresAt = null,
    int CredentialVersion = 0);

public sealed record AccessibleBranchData(
    int Id,
    string Code,
    string Name);

public sealed record AuthCurrentUserData(
    int Id,
    string Username,
    string RoleCode,
    bool MustChangePassword = false);
