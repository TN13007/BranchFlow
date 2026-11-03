using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BranchFlow.Web.Services.Auth;

namespace BranchFlow.Web.Services.Api;

public sealed class SessionApiTransport(IHttpClientFactory factory, WebSessionStore store, AuthApiClient auth, TimeProvider clock)
{
    public async Task<T> SendAsync<T>(string key, HttpMethod method, string uri, object? body = null, CancellationToken ct = default)
    {
        var session = store.Find(key) ?? throw new ApiException(HttpStatusCode.Unauthorized, "Vui lòng đăng nhập lại.");
        // Cùng khóa cho refresh/logout và request; không dùng lại refresh token khi gọi đồng thời.
        await session.Gate.WaitAsync(ct);
        try
        {
            if (store.Find(key) is null) throw new ApiException(HttpStatusCode.Unauthorized, "Vui lòng đăng nhập lại.");
            var refreshed = false;
            if (session.Tokens.AccessTokenExpiresAt <= clock.GetUtcNow().AddSeconds(30))
            {
                await RefreshAsync(key, session, ct);
                refreshed = true;
            }
            using var client = factory.CreateClient("BranchFlowApi");
            using var response = await SendOnceAsync(client, session.Tokens.AccessToken, method, uri, body, ct);
            if (method == HttpMethod.Post && uri == "api/orders" && response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Created)
                throw new ApiException(HttpStatusCode.BadGateway, "Dịch vụ chưa xác nhận tạo đơn đúng hợp đồng. Hãy kiểm tra danh sách đơn.");
            // Chỉ GET an toàn được gửi lại; POST tạo đơn không bao giờ tự replay.
            if (response.StatusCode == HttpStatusCode.Unauthorized && method == HttpMethod.Get && !refreshed)
            {
                await RefreshAsync(key, session, ct);
                using var retry = await SendOnceAsync(client, session.Tokens.AccessToken, method, uri, body, ct);
                return await ReadAsync<T>(key, retry, ct);
            }
            return await ReadAsync<T>(key, response, ct);
        }
        finally { session.Gate.Release(); }
    }

    private async Task RefreshAsync(string key, WebSession session, CancellationToken ct)
    {
        try
        {
            var tokens = await auth.RefreshAsync(session.Tokens.RefreshToken, ct);
            if (!store.ValidTokens(tokens) || store.Find(key) is null)
                throw new ApiException(HttpStatusCode.Unauthorized, "Phiên đã hết hạn. Vui lòng đăng nhập lại.");
            session.Tokens = tokens;
        }
        catch (ApiException)
        {
            store.Remove(key);
            throw new ApiException(HttpStatusCode.Unauthorized, "Phiên đã hết hạn. Vui lòng đăng nhập lại.");
        }
        catch (HttpRequestException)
        {
            // Refresh có thể đã được rotate ở API dù response bị mất: buộc đăng nhập lại.
            store.Remove(key);
            throw new ApiException(HttpStatusCode.Unauthorized, "Không thể gia hạn phiên. Vui lòng đăng nhập lại.");
        }
        catch (OperationCanceledException) { store.Remove(key); throw; }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            store.Remove(key);
            throw new ApiException(HttpStatusCode.Unauthorized, "Phản hồi gia hạn không hợp lệ. Vui lòng đăng nhập lại.");
        }
    }

    private async Task<T> ReadAsync<T>(string key, HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized) store.Remove(key);
        if (!response.IsSuccessStatusCode) throw await ApiException.FromResponseAsync(response, ct);
        try { return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new ApiException(HttpStatusCode.BadGateway, "Dịch vụ trả dữ liệu không hợp lệ."); }
        catch (JsonException) { throw new ApiException(HttpStatusCode.BadGateway, "Dịch vụ trả dữ liệu không hợp lệ."); }
    }

    private static async Task<HttpResponseMessage> SendOnceAsync(HttpClient client, string token, HttpMethod method, string uri, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request, ct);
    }
}
