namespace BranchFlow.Infrastructure.Repositories;

public sealed record AuthUserData(
    int Id,
    string Username,
    string PasswordHash,
    string FullName,
    string RoleCode);

public sealed record AccessibleBranchData(
    int Id,
    string Code,
    string Name);
