using BranchFlow.Web.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace BranchFlow.Web.Services.Api;

public sealed class ManagementApiClient(AuthenticationStateProvider state, SessionApiTransport transport)
{
    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? input = null)
    {
        var key = (await state.GetAuthenticationStateAsync()).User.FindFirst(WebSessionStore.SessionClaim)?.Value
            ?? throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Vui lòng đăng nhập lại.");
        return await transport.SendAsync<T>(key, method, "api/management/" + path, input);
    }
    public Task<List<T>> ListAsync<T>(string path) => SendAsync<List<T>>(HttpMethod.Get, path);
    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);
    public Task<int> SaveAsync(string path, int id, object input) => id == 0
        ? SendAsync<int>(HttpMethod.Post, path, input)
        : PutAsync(path + "/" + id, input);
    public Task<int> PutAsync(string path, object input) => SendAsync<int>(HttpMethod.Put, path, input);
    public Task<int> PostAsync(string path, object input) => SendAsync<int>(HttpMethod.Post, path, input);
}
