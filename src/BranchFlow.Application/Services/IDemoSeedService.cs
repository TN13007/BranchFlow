namespace BranchFlow.Application.Services;

public interface IDemoSeedService
{
    Task<DemoSeedResult> SeedAsync(
        string password,
        CancellationToken cancellationToken = default);
}

public sealed record DemoSeedAccountResult(
    string Username,
    int UserId,
    string RoleCode,
    bool IsActive);

public sealed record DemoSeedBranchResult(
    int Id,
    string Code);

public sealed record DemoSeedResult(
    IReadOnlyList<DemoSeedAccountResult> Accounts,
    IReadOnlyList<DemoSeedBranchResult> Branches);
