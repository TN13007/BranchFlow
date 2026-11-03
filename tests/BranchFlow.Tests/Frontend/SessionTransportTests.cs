using System.Net;
using System.Net.Http.Json;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Web.Services.Api;
using BranchFlow.Web.Services.Auth;

namespace BranchFlow.Tests.Frontend;

public sealed class SessionTransportTests
{
    [Fact]
    public async Task CreateRequires201AndUsesServerResult()
    {
        string? payload = null;
        using var handler = new Handler(async req =>
        {
            payload = await req.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(new CreatedOrder(42, "Q1-20261103-0001", 1, new(2026, 11, 3), 123456, "COMPLETED", DateTimeOffset.UtcNow)) };
        });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        var response = await transport.SendAsync<CreatedOrder>(key, HttpMethod.Post, "api/orders", new CreateOrderRequest(1, [new(101, 2, [], [])]));
        Assert.Equal(123456, response.TotalAmount); Assert.Equal(42, response.Id);
        Assert.DoesNotContain("price", payload!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("total", payload!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create200IsNotReportedAsSuccessfulCreation()
    {
        using var handler = new Handler(_ => Task.FromResult(Json(new { id = 42 })));
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        var error = await Assert.ThrowsAsync<ApiException>(() => transport.SendAsync<object>(key, HttpMethod.Post, "api/orders", new { }));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
    }

    [Fact]
    public async Task AmbiguousRefreshFailureInvalidatesLocalSession()
    {
        using var handler = new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one", true));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        var error = await Assert.ThrowsAsync<ApiException>(() => transport.SendAsync<object>(key, HttpMethod.Get, "api/order-menu"));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode); Assert.Null(store.Find(key));
    }

    [Fact]
    public void MoneyAndDateDisplayPreserveFractionAndVietnamTimezone()
    {
        Assert.Equal("30.000,50đ", BranchFlow.Web.Components.Orders.Display.Money(30000.50m));
        Assert.Equal("04/11/2026 00:00", BranchFlow.Web.Components.Orders.Display.VietnamTime(new DateTimeOffset(2026, 11, 3, 17, 0, 0, TimeSpan.Zero)));
    }

    private static TokenPair Tokens(string marker, bool expiresSoon = false) => new()
    {
        AccessToken = "access-" + marker, RefreshToken = "refresh-" + marker,
        AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresSoon ? 5 : 900), RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
    };
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => send(request);
    }
    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, false) { BaseAddress = new Uri("https://contract.test/") };
    }
    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    [Fact]
    public async Task ConcurrentCallsRotateOnceAndUseNewToken()
    {
        int refreshes = 0; var seen = new List<string?>();
        using var handler = new Handler(async req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("refresh"))
            { Interlocked.Increment(ref refreshes); await Task.Yield(); return Json(Tokens("rotated")); }
            seen.Add(req.Headers.Authorization?.Parameter); return Json(new { ok = true });
        });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("initial", true));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => transport.SendAsync<object>(key, HttpMethod.Get, "api/order-menu")));
        Assert.Equal(1, refreshes); Assert.Equal(8, seen.Count); Assert.All(seen, t => Assert.Equal("access-rotated", t));
    }

    [Fact]
    public async Task SeparateUsersNeverShareAccessTokens()
    {
        var seen = new List<string?>();
        using var handler = new Handler(req => { lock (seen) seen.Add(req.Headers.Authorization?.Parameter); return Task.FromResult(Json(new { ok = true })); });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var one = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one")); var two = store.Add(new(2, "two", "EMPLOYEE"), Tokens("two"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        await Task.WhenAll(transport.SendAsync<object>(one, HttpMethod.Get, "api/branches/accessible"), transport.SendAsync<object>(two, HttpMethod.Get, "api/branches/accessible"));
        Assert.Contains("access-one", seen); Assert.Contains("access-two", seen);
        store.Remove(one); Assert.Null(store.Find(one)); Assert.NotNull(store.Find(two));
    }

    [Theory]
    [InlineData(403)] [InlineData(500)]
    public async Task DoesNotRefreshOrReplayForbiddenOrServerError(int status)
    {
        var calls = 0;
        using var handler = new Handler(_ => { calls++; return Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)); });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        var ex = await Assert.ThrowsAsync<ApiException>(() => transport.SendAsync<object>(key, HttpMethod.Get, "api/order-menu"));
        Assert.Equal((HttpStatusCode)status, ex.StatusCode); Assert.Equal(1, calls); Assert.NotNull(store.Find(key));
    }

    [Fact]
    public async Task Get401RefreshesAtMostOnceThenInvalidatesSession()
    {
        int gets = 0, refreshes = 0;
        using var handler = new Handler(req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("refresh")) { refreshes++; return Task.FromResult(Json(Tokens("rotated"))); }
            gets++; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        await Assert.ThrowsAsync<ApiException>(() => transport.SendAsync<object>(key, HttpMethod.Get, "api/order-menu"));
        Assert.Equal(2, gets); Assert.Equal(1, refreshes); Assert.Null(store.Find(key));
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task PostIsNeverReplayedAfterTimeoutOr401(bool timeout)
    {
        var calls = 0;
        using var handler = new Handler(_ =>
        {
            calls++;
            if (timeout) throw new TaskCanceledException();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        });
        var factory = new Factory(handler); using var store = new WebSessionStore(TimeProvider.System);
        var key = store.Add(new(1, "one", "EMPLOYEE"), Tokens("one"));
        var transport = new SessionApiTransport(factory, store, new(factory), TimeProvider.System);
        await Assert.ThrowsAnyAsync<Exception>(() => transport.SendAsync<object>(key, HttpMethod.Post, "api/orders", new { }));
        Assert.Equal(1, calls);
    }
}
