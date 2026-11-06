using BranchFlow.Application.DTOs.Sprint4;
using Microsoft.JSInterop;

namespace BranchFlow.Web.Models.Cart;

// Chỉ lưu ID/số lượng trong tab hiện tại, theo user; không lưu token hay giá tin cậy.
public sealed class CartDraftStore(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? module;
    private int ownerId;
    private string? saved;
    public sealed class Draft
    {
        public int BranchId { get; set; }
        public bool OutcomeUnknown { get; set; }
        public List<CreateOrderItem> Items { get; set; } = [];
    }

    public async Task RestoreAsync(int userId, CartState cart, OrderMenu menu)
    {
        ownerId = userId;
        if (cart.Lines.Count > 0 || cart.OutcomeUnknown) return;
        module ??= await js.InvokeAsync<IJSObjectReference>("import", "/js/order-draft.js");
        var draft = await module.InvokeAsync<Draft?>("read", ownerId);
        if (draft is null || draft.BranchId != menu.Branch.Id) return;
        cart.SetBranch(draft.BranchId);
        try
        {
            foreach (var item in draft.Items) cart.Save(menu, item);
        }
        finally
        {
            // Giữ cờ chưa rõ kết quả ngay cả khi menu hiện tại không còn món cũ.
            if (draft.OutcomeUnknown) cart.MarkUnknown();
        }
    }

    public async Task<bool> SaveAsync(CartState cart)
    {
        if (ownerId <= 0) return true; // Preview không có giỏ của tài khoản thật.
        var draft = new Draft { BranchId = cart.BranchId ?? 0, OutcomeUnknown = cart.OutcomeUnknown,
            Items = cart.Lines.Select(x => x.Request).ToList() };
        var fingerprint = System.Text.Json.JsonSerializer.Serialize(draft);
        if (fingerprint == saved) return true;
        try
        {
            module ??= await js.InvokeAsync<IJSObjectReference>("import", "/js/order-draft.js");
            if (!await module.InvokeAsync<bool>("save", ownerId, draft)) return false;
            saved = fingerprint;
            return true;
        }
        catch (Exception ex) when (ex is JSException or OperationCanceledException) { return false; }
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null) try { await module.DisposeAsync(); }
        catch (Exception ex) when (ex is JSDisconnectedException or OperationCanceledException) { }
    }
}
