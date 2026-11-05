using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using BranchFlow.Web.Models.Cart;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class OrderServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 11, 4, 18, 0, 0, TimeSpan.Zero);

    private static readonly CreateOrderRequest ValidRequest =
        new(1, [new(101, 2, [new(5, 3), new(6, 1)], [7, 9])]);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static (
        Mock<IOrderRepository> Orders,
        Mock<IOrderMenuService> Menus,
        Mock<IBranchAccessService> Access,
        Mock<IUnitOfWork> Work,
        OrderService Service) CreateService()
    {
        var orders = new Mock<IOrderRepository>(MockBehavior.Strict);
        var menus = new Mock<IOrderMenuService>(MockBehavior.Strict);
        var access = new Mock<IBranchAccessService>(MockBehavior.Strict);
        var work = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var service = new OrderService(
            orders.Object,
            menus.Object,
            access.Object,
            work.Object,
            new FixedTimeProvider(FixedNow));

        return (orders, menus, access, work, service);
    }

    private static void SetupTransaction(Mock<IUnitOfWork> work)
    {
        work.Setup(w => w.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        work.Setup(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        work.Setup(w => w.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateAsyncValidRequestAllocatesCodeAfterValidationAndCommits()
    {
        var (orders, menus, access, work, service) = CreateService();
        var calls = new List<string>();
        SalesOrder? capturedOrder = null;

        work.Setup(w => w.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Begin"))
            .Returns(Task.CompletedTask);

        menus.Setup(m => m.GetMenuByBranchAsync(7, 1, It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Menu"))
            .ReturnsAsync(PreviewMenu.Create());

        orders.Setup(o => o.AllocateCodeAsync(1, new DateOnly(2026, 11, 5), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("AllocateCode"))
            .ReturnsAsync(("CN001", 1));

        orders.Setup(o => o.Add(It.IsAny<SalesOrder>()))
            .Callback<SalesOrder>(order =>
            {
                calls.Add("Add");
                capturedOrder = order;
            });

        work.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Save"))
            .ReturnsAsync(1);

        work.Setup(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("Commit"))
            .Returns(Task.CompletedTask);

        var result = await service.CreateAsync(7, ValidRequest);

        Assert.NotNull(capturedOrder);
        Assert.Equal("CN001", result.Code);
        Assert.Equal(1, result.BranchId);
        Assert.Equal(new DateOnly(2026, 11, 5), result.BusinessDate);
        Assert.Equal(98000m, result.TotalAmount);
        Assert.Equal("COMPLETED", result.Status);
        Assert.Equal(FixedNow, result.CreatedAt);

        Assert.Equal("CN001", capturedOrder.Code);
        Assert.Equal(1, capturedOrder.DailySequence);
        Assert.Equal(7, capturedOrder.CreatedByUserId);
        Assert.Equal(FixedNow.UtcDateTime, capturedOrder.CreatedAt);
        Assert.Equal(98000m, capturedOrder.TotalAmount);
        Assert.Equal("COMPLETED", capturedOrder.Status);

        var item = Assert.Single(capturedOrder.OrderItems);
        Assert.Equal("Trà đào cam sả", item.ProductNameSnapshot);
        Assert.Equal("M", item.SizeNameSnapshot);
        Assert.Equal(30000m, item.UnitPriceSnapshot);
        Assert.Equal(98000m, item.SubtotalAmount);
        Assert.Equal(2, item.OrderItemToppings.Count);
        Assert.Equal(2, item.OrderItemNotes.Count);

        Assert.Equal(
            ["Begin", "Menu", "AllocateCode", "Add", "Save", "Commit"],
            calls);

        access.VerifyNoOtherCalls();
        orders.VerifyAll();
        menus.VerifyAll();
        work.VerifyAll();
    }

    [Fact]
    public async Task CreateAsyncUnauthorizedBranchRollsBackWithoutAllocatingCode()
    {
        var (orders, menus, access, work, service) = CreateService();
        SetupTransaction(work);

        menus.Setup(m => m.GetMenuByBranchAsync(7, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Không có quyền truy cập chi nhánh."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateAsync(7, ValidRequest));

        menus.VerifyAll();
        work.Verify(w => w.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        orders.Verify(o => o.AllocateCodeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        orders.Verify(o => o.Add(It.IsAny<SalesOrder>()), Times.Never);
        work.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        work.Verify(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsyncUnavailableBranchRollsBackWithoutAllocatingCode()
    {
        var (orders, menus, access, work, service) = CreateService();
        SetupTransaction(work);

        menus.Setup(m => m.GetMenuByBranchAsync(7, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderMenu?)null);

        await Assert.ThrowsAsync<OrderBranchUnavailableException>(
            () => service.CreateAsync(7, ValidRequest));

        menus.VerifyAll();
        work.Verify(w => w.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        orders.Verify(o => o.AllocateCodeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        orders.Verify(o => o.Add(It.IsAny<SalesOrder>()), Times.Never);
        work.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        work.Verify(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsyncInvalidOrderRollsBackBeforeAllocatingCode()
    {
        var (orders, menus, access, work, service) = CreateService();
        SetupTransaction(work);

        menus.Setup(m => m.GetMenuByBranchAsync(7, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreviewMenu.Create());

        var invalidRequest = new CreateOrderRequest(
            1,
            [new CreateOrderItem(9999, 1, [], [])]);

        await Assert.ThrowsAsync<OrderValidationException>(
            () => service.CreateAsync(7, invalidRequest));

        menus.VerifyAll();
        work.Verify(w => w.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        orders.Verify(o => o.AllocateCodeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        orders.Verify(o => o.Add(It.IsAny<SalesOrder>()), Times.Never);
        work.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        work.Verify(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsyncSaveFailureRollsBackAndDoesNotCommit()
    {
        var (orders, menus, access, work, service) = CreateService();
        SetupTransaction(work);

        menus.Setup(m => m.GetMenuByBranchAsync(7, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PreviewMenu.Create());

        orders.Setup(o => o.AllocateCodeAsync(1, new DateOnly(2026, 11, 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("CN001", 1));

        orders.Setup(o => o.Add(It.IsAny<SalesOrder>()));

        work.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated database save failure."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(7, ValidRequest));

        menus.VerifyAll();
        orders.VerifyAll();
        work.Verify(w => w.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        work.Verify(w => w.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        access.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsyncRejectsInvalidBranchBeforeStartingTransaction()
    {
        var (orders, menus, access, work, service) = CreateService();

        await Assert.ThrowsAsync<OrderValidationException>(
            () => service.CreateAsync(7, new CreateOrderRequest(0, [ValidRequest.Items[0]])));

        work.Verify(w => w.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        menus.VerifyNoOtherCalls();
        orders.VerifyNoOtherCalls();
        access.VerifyNoOtherCalls();
        work.VerifyNoOtherCalls();
    }
}