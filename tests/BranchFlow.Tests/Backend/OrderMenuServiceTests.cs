using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Repositories;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class OrderMenuServiceTests
{
    [Fact]
    public async Task WrongBranchDoesNotReadMenu()
    {
        var repository = new Mock<IOrderMenuRepository>(MockBehavior.Strict);
        var access = new Mock<IMenuAccessRepository>();
        access.Setup(a => a.CanReadMenuAsync(7, 2, default)).ReturnsAsync(false);
        var service = new OrderMenuService(repository.Object, access.Object);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetMenuByBranchAsync(7, 2));
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MapsDatabasePriceAndProductSizeIdWithoutChangingContract()
    {
        var repository = new Mock<IOrderMenuRepository>();
        var access = new Mock<IMenuAccessRepository>();
        access.Setup(a => a.CanReadMenuAsync(7, 2, default)).ReturnsAsync(true);
        repository.Setup(r => r.GetMenuByBranchAsync(2, default)).ReturnsAsync(new OrderMenuData(
            new(2, "Q2", "Chi nhánh 2"),
            [new(11, "Trà", [new(12, "Trà sữa", null, 30000m, [new(101, 1, "M", 30000m)])])],
            [new(3, "Topping", [new(5, "Trân châu", 5000m, 3)])],
            [new(4, "Đường", 1, [new(8, "Ít đường")])]));
        var menu = await new OrderMenuService(repository.Object, access.Object).GetMenuByBranchAsync(7, 2);
        Assert.NotNull(menu);
        var size = Assert.Single(Assert.Single(Assert.Single(menu.Categories).Products).ProductSizes);
        Assert.Equal(101, size.Id);
        Assert.Equal(30000m, size.Price);
        Assert.Equal(3, Assert.Single(Assert.Single(menu.ToppingGroups).Toppings).MaxQuantity);
        Assert.Equal(1, Assert.Single(menu.NoteGroups).MaxSelections);
    }
}
