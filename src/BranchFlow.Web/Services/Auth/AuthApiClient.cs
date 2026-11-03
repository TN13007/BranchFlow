using System.Net.Http.Headers;
using System.Net.Http.Json;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Web.Services.Api;

namespace BranchFlow.Web.Services.Auth;

public sealed class AuthApiClient(IHttpClientFactory factory)
{
    public async Task<TokenPair> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        using var client = factory.CreateClient("BranchFlowApi");
        using var response = await client.PostAsJsonAsync("api/auth/login", request, ct);
        if (!response.IsSuccessStatusCode) throw await ApiException.FromResponseAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TokenPair>(ct) ?? throw new InvalidOperationException("API không trả token.");
    }

    public async Task<TokenPair> RefreshAsync(string token, CancellationToken ct = default)
    {
        using var client = factory.CreateClient("BranchFlowApi");
        using var response = await client.PostAsJsonAsync("api/auth/refresh", new RefreshRequest(token), ct);
        if (!response.IsSuccessStatusCode) throw await ApiException.FromResponseAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<TokenPair>(ct) ?? throw new InvalidOperationException("API không trả token.");
    }

    public async Task<CurrentUser> MeAsync(string accessToken, CancellationToken ct = default)
    {
        using var client = factory.CreateClient("BranchFlowApi");
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw await ApiException.FromResponseAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<CurrentUser>(ct) ?? throw new InvalidOperationException("API không trả user.");
    }

    public async Task LogoutAsync(string accessToken, CancellationToken ct = default)
    {
        using var client = factory.CreateClient("BranchFlowApi");
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw await ApiException.FromResponseAsync(response, ct);
    }
}
