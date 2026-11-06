namespace BranchFlow.Api.Security;

internal sealed class FixedUtcTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
