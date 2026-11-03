using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Web.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace BranchFlow.Web.Services.Api;

public sealed class OrderApiClient(AuthenticationStateProvider state, SessionApiTransport transport)
{
    private async Task<string> KeyAsync() => (await state.GetAuthenticationStateAsync()).User.FindFirst(WebSessionStore.SessionClaim)?.Value
        ?? throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Vui lòng đăng nhập lại.");

    public async Task<CurrentUser> MeAsync() => await transport.SendAsync<CurrentUser>(await KeyAsync(), HttpMethod.Get, "api/auth/me");
    public async Task<List<AccessibleBranch>> BranchesAsync() => await transport.SendAsync<List<AccessibleBranch>>(await KeyAsync(), HttpMethod.Get, "api/branches/accessible");
    public async Task<OrderMenu> MenuAsync(int branchId) => await transport.SendAsync<OrderMenu>(await KeyAsync(), HttpMethod.Get, $"api/order-menu?branchId={branchId}");
    public async Task<CreatedOrder> CreateAsync(CreateOrderRequest request) => await transport.SendAsync<CreatedOrder>(await KeyAsync(), HttpMethod.Post, "api/orders", request);
    public async Task<List<OrderSummary>> MineAsync(string keyword, DateOnly? date) => await transport.SendAsync<List<OrderSummary>>(await KeyAsync(), HttpMethod.Get,
        $"api/orders/mine?keyword={Uri.EscapeDataString(keyword)}" + (date.HasValue ? $"&businessDate={date:yyyy-MM-dd}" : ""));
    public async Task<OrderDetail> DetailAsync(int id) => await transport.SendAsync<OrderDetail>(await KeyAsync(), HttpMethod.Get, $"api/orders/{id}");
}
