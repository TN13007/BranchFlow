using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace BranchFlow.Web.Services.Auth;

public sealed class SessionAuthenticationStateProvider(ILoggerFactory loggerFactory, WebSessionStore store)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(15);
    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState state, CancellationToken cancellationToken)
    {
        var session = store.Find(state.User.FindFirst(WebSessionStore.SessionClaim)?.Value);
        return Task.FromResult(session is not null && state.User.Identity?.Name == session.User.Username);
    }
}
