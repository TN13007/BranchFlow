using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Web.Models.Cart;

namespace BranchFlow.Tests.Backend;

public class OrderPricingTests
{
    private static CreateOrderItem Item(int cups = 2) => new(101,cups,[new(5,3),new(6,1)],[7,9]);

    [Fact]
    public void ServerPricesMultipleToppingsPerCupAndStoresSnapshots()
    {
        var item = Assert.Single(OrderService.PriceItems(PreviewMenu.Create(),new(1,[Item()])));
        Assert.Equal(98000m,item.SubtotalAmount);
        Assert.Equal(30000m,item.UnitPriceSnapshot);
        Assert.Equal(2,item.OrderItemToppings.Count);
        Assert.Equal(2,item.OrderItemNotes.Count);
        Assert.Equal("Trà đào cam sả",item.ProductNameSnapshot);
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(1000)]
    public void RejectsInvalidCupQuantity(int quantity) => Assert.Throws<OrderValidationException>(() => OrderService.PriceItems(PreviewMenu.Create(),new(1,[Item(quantity)])));

    [Fact]
    public void RejectsDuplicateToppingAndMoreThanThreePortions()
    {
        var duplicate=Item() with {Toppings=[new(5,1),new(5,2)]};
        var excessive=Item() with {Toppings=[new(5,4)]};
        Assert.Throws<OrderValidationException>(()=>OrderService.PriceItems(PreviewMenu.Create(),new(1,[duplicate])));
        Assert.Throws<OrderValidationException>(()=>OrderService.PriceItems(PreviewMenu.Create(),new(1,[excessive])));
    }

    [Fact]
    public void RejectsTwoNotesInSameGroupAndMissingSize()
    {
        Assert.Throws<OrderValidationException>(()=>OrderService.PriceItems(PreviewMenu.Create(),new(1,[Item() with {NoteOptionIds=[7,8]}])));
        Assert.Throws<OrderValidationException>(()=>OrderService.PriceItems(PreviewMenu.Create(),new(1,[Item() with {ProductSizeId=9999}])));
    }
}
