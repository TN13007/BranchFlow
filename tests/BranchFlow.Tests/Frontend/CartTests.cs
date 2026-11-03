using System.Text.Json;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Web.Models.Cart;

namespace BranchFlow.Tests.Frontend;

public sealed class CartTests
{
    private static CreateOrderItem Item(int quantity = 2) => new(101, quantity, [new(5, 3), new(6, 1)], [7]);
    private static CartState Cart() { var cart = new CartState(); cart.SetBranch(1); return cart; }

    [Fact]
    public void PreviewUsesToppingQuantityPerCupAndRequestContainsOnlyContractFields()
    {
        var cart = Cart(); cart.Save(PreviewMenu.Create(), Item());
        Assert.Equal(98000, cart.PreviewTotal);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(cart.ToRequest(PreviewMenu.Create()), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Equal(["branchId", "items"], json.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        var line = json.RootElement.GetProperty("items")[0];
        Assert.Equal(["productSizeId", "quantity", "toppings", "noteOptionIds"], line.EnumerateObject().Select(p => p.Name).ToArray());
        Assert.DoesNotContain("price", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)] [InlineData(4)] [InlineData(-1)]
    public void RejectsInvalidToppingQuantity(int quantity)
        => Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item() with { Toppings = [new(5, quantity)] }));

    [Fact]
    public void RejectsDuplicateIdsAndTwoNotesFromSameGroup()
    {
        Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item() with { Toppings = [new(5, 1), new(5, 2)] }));
        Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item() with { NoteOptionIds = [7, 7] }));
        Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item() with { NoteOptionIds = [7, 8] }));
    }

    [Fact]
    public void KeepsDifferentConfigurationsAndEditsOnlyChosenLine()
    {
        var cart = Cart(); var menu = PreviewMenu.Create();
        cart.Save(menu, Item()); cart.Save(menu, Item() with { ProductSizeId = 102 });
        var key = cart.Lines[0].Key;
        cart.Save(menu, Item(1), key);
        Assert.Equal(2, cart.Lines.Count); Assert.Equal(102, cart.Lines[1].Request.ProductSizeId);
        Assert.Equal(1, cart.Lines[0].Request.Quantity);
        cart.Remove(key); Assert.Single(cart.Lines);
    }

    [Fact]
    public void DisabledMenuBlocksSubmitAndRetainsOriginalCart()
    {
        var cart = Cart(); cart.Save(PreviewMenu.Create(), Item());
        var menu = PreviewMenu.Create() with { Categories = [] };
        Assert.Throws<InvalidOperationException>(() => cart.ToRequest(menu));
        Assert.Single(cart.Lines); Assert.Equal(98000, cart.PreviewTotal);
    }

    [Fact]
    public void PriceChangeUpdatesPreviewButDoesNotAlterPayload()
    {
        var menu = PreviewMenu.Create(); var cart = Cart(); cart.Save(menu, Item());
        menu.Categories[0].Products[0].ProductSizes[0] = new(101, 1, "M", 40000);
        var request = cart.ToRequest(menu);
        Assert.Equal(118000, cart.PreviewTotal); Assert.Equal(Item().ProductSizeId, request.Items[0].ProductSizeId);
    }

    [Fact]
    public void UnknownOutcomeLocksChangesUntilExplicitAcknowledgment()
    {
        var cart = Cart(); var menu = PreviewMenu.Create(); cart.Save(menu, Item()); cart.MarkUnknown();
        Assert.Throws<InvalidOperationException>(() => cart.ToRequest(menu));
        Assert.Throws<InvalidOperationException>(() => cart.SetBranch(2));
        Assert.Throws<InvalidOperationException>(() => cart.Save(menu, Item()));
        cart.Remove(cart.Lines[0].Key); Assert.Single(cart.Lines);
        cart.AcknowledgeCheckedHistory(); Assert.Single(cart.ToRequest(menu).Items);
    }

    [Fact]
    public void RejectsMissingSizeZeroCupsAndEmptyCart()
    {
        Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item() with { ProductSizeId = 0 }));
        Assert.Throws<InvalidOperationException>(() => Cart().Save(PreviewMenu.Create(), Item(0)));
        Assert.Throws<InvalidOperationException>(() => Cart().ToRequest(PreviewMenu.Create()));
    }
}
