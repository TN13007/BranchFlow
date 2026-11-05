namespace BranchFlow.Application.Services;

public interface ISalesDemoSeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}