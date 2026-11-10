using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpOrderAdjustmentTests
{
    private const int ActorId = 10;
    private const int OrderId = 100;
    private const int BranchId = 1;

    private static readonly DateTimeOffset FixedNow =
        new(2026, 11, 12, 10, 0, 0, TimeSpan.Zero);

    private static readonly byte[] ValidVersion = [1, 2, 3, 4, 5, 6, 7, 8];

    private readonly Mock<IMvpRepository> repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> work = new(MockBehavior.Strict);
    private readonly Mock<IBranchAccessService> access = new(MockBehavior.Strict);
    private readonly Mock<IPasswordHasher<AppUser>> hasher = new();

    private OrderAdjustment? capturedAdjustment;

    private MvpService CreateService() => new(
        repo.Object,
        work.Object,
        access.Object,
        hasher.Object,
        new FixedTimeProvider(FixedNow));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private SalesOrder CreateOrder(string status = "COMPLETED")
    {
        var item = new OrderItem
        {
            Id = 501,
            SalesOrderId = OrderId,
            ProductSizeId = 101,
            ProductNameSnapshot = "Trà đào",
            SizeNameSnapshot = "M",
            Quantity = 2,
            UnitPriceSnapshot = 30000m,
            SubtotalAmount = 70000m,
            OrderItemToppings =
            [
                new OrderItemTopping
                {
                    Id = 1,
                    OrderItemId = 501,
                    ToppingId = 5,
                    ToppingNameSnapshot = "Trân châu",
                    Quantity = 1,
                    UnitPriceSnapshot = 5000m
                }
            ],
            OrderItemNotes =
            [
                new OrderItemNote
                {
                    Id = 1,
                    OrderItemId = 501,
                    NoteOptionId = 7,
                    NoteNameSnapshot = "Ít đá"
                }
            ]
        };

        return new SalesOrder
        {
            Id = OrderId,
            Code = "CN001",
            BranchId = BranchId,
            CreatedByUserId = 20,
            Status = status,
            TotalAmount = 70000m,
            CreatedAt = FixedNow.UtcDateTime,
            RowVersion = ValidVersion,
            OrderItems = [item]
        };
    }

    private void SetupOrder(SalesOrder order)
    {
        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        repo.Setup(x => x.ExpectVersion(
            order, It.Is<byte[]>(v => v.SequenceEqual(ValidVersion))));

        repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity =>
            {
                if (entity is OrderAdjustment adjustment)
                    capturedAdjustment = adjustment;
            });

        work.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        work.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        work.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        work.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static AdjustOrderRequest ValidRequest(int quantity = 3) => new()
    {
        Reason = "Khách yêu cầu thay đổi số lượng",
        RowVersion = Convert.ToBase64String(ValidVersion),
        Cancel = false,
        Items = [new QuantityChange { ItemId = 501, Quantity = quantity }]
    };

    [Fact]
    public async Task AdjustOrderAsync_ValidQuantity_UpdatesSubtotalAndTotal()
    {
        var order = CreateOrder();
        SetupOrder(order);

        var service = CreateService();

        var result = await service.AdjustOrderAsync(
            ActorId, OrderId, ValidRequest(3), CancellationToken.None);

        Assert.Equal(OrderId, result);
        Assert.Equal(3, order.OrderItems.Single().Quantity);
        Assert.Equal(105000m, order.OrderItems.Single().SubtotalAmount);
        Assert.Equal(105000m, order.TotalAmount);
        Assert.Equal(FixedNow.UtcDateTime, order.UpdatedAt);

        Assert.NotNull(capturedAdjustment);
        Assert.Equal(OrderId, capturedAdjustment.SalesOrderId);
        Assert.Equal(ActorId, capturedAdjustment.AdjustedByUserId);
        Assert.Equal("Khách yêu cầu thay đổi số lượng", capturedAdjustment.Reason);

        work.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_ValidQuantity_RecordsBeforeAndAfterSnapshots()
    {
        var order = CreateOrder();
        SetupOrder(order);

        var service = CreateService();

        await service.AdjustOrderAsync(
            ActorId, OrderId, ValidRequest(3), CancellationToken.None);

        Assert.NotNull(capturedAdjustment);

        using var before = System.Text.Json.JsonDocument.Parse(
            capturedAdjustment.BeforeData);

        using var after = System.Text.Json.JsonDocument.Parse(
            capturedAdjustment.AfterData);

        Assert.Equal(
            70000m,
            before.RootElement.GetProperty("TotalAmount").GetDecimal());

        Assert.Equal(
            105000m,
            after.RootElement.GetProperty("TotalAmount").GetDecimal());

        var beforeItem = before.RootElement
            .GetProperty("Items")[0];

        var afterItem = after.RootElement
            .GetProperty("Items")[0];

        Assert.Equal(2, beforeItem.GetProperty("Quantity").GetInt32());
        Assert.Equal(3, afterItem.GetProperty("Quantity").GetInt32());

        Assert.Equal(
            "Trân châu",
            afterItem.GetProperty("Toppings")[0]
                .GetProperty("ToppingNameSnapshot").GetString());

        Assert.Equal(
            "Ít đá",
            afterItem.GetProperty("Notes")[0]
                .GetProperty("NoteNameSnapshot").GetString());
    }

    [Fact]
    public async Task AdjustOrderAsync_CancelRequest_ChangesStatusAndRecordsReason()
    {
        var order = CreateOrder();
        SetupOrder(order);

        var request = new AdjustOrderRequest
        {
            Reason = "Khách hàng yêu cầu hủy đơn",
            RowVersion = Convert.ToBase64String(ValidVersion),
            Cancel = true,
            Items = []
        };

        var service = CreateService();

        var result = await service.AdjustOrderAsync(
            ActorId, OrderId, request, CancellationToken.None);

        Assert.Equal(OrderId, result);
        Assert.Equal("CANCELLED", order.Status);
        Assert.Equal(70000m, order.TotalAmount);

        Assert.NotNull(capturedAdjustment);
        Assert.Equal("Khách hàng yêu cầu hủy đơn", capturedAdjustment.Reason);

        using var after = System.Text.Json.JsonDocument.Parse(
            capturedAdjustment.AfterData);

        Assert.Equal(
            "CANCELLED",
            after.RootElement.GetProperty("Status").GetString());

        work.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_CancelledOrder_ThrowsInvalidOperationException()
    {
        var order = CreateOrder("CANCELLED");

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, ValidRequest(), CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.VerifyAll();
        access.VerifyAll();
    }

    [Fact]
    public async Task AdjustOrderAsync_InvalidRowVersion_ThrowsArgumentException()
    {
        var order = CreateOrder();

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = ValidRequest();
        request.RowVersion = "invalid-version";

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, request, CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.Verify(x => x.ExpectVersion(
            It.IsAny<SalesOrder>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_ActorCannotAccessBranch_ThrowsUnauthorizedAccessException()
    {
        var order = CreateOrder();

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, ValidRequest(), CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.Verify(x => x.ExpectVersion(
            It.IsAny<SalesOrder>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_SaveChangesFails_RollsBackTransaction()
    {
        var order = CreateOrder();
        SetupOrder(order);

        work.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated database failure."));

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, ValidRequest(), CancellationToken.None));

        work.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        work.Verify(x => x.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        work.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000)]
    public async Task AdjustOrderAsync_InvalidQuantity_ThrowsArgumentException(int quantity)
    {
        var order = CreateOrder();

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, ValidRequest(quantity), CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.Verify(x => x.ExpectVersion(
            It.IsAny<SalesOrder>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_ItemDoesNotBelongToOrder_ThrowsArgumentException()
    {
        var order = CreateOrder();

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = ValidRequest();
        request.Items = [new QuantityChange { ItemId = 9999, Quantity = 3 }];

        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, request, CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.Verify(x => x.ExpectVersion(
            It.IsAny<SalesOrder>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task AdjustOrderAsync_EmptyReason_ThrowsArgumentException()
    {
        var order = CreateOrder();

        access.Setup(x => x.CanAccessBranchAsync(
                ActorId, BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repo.Setup(x => x.OrderAsync(OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = ValidRequest();
        request.Reason = "   ";

        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AdjustOrderAsync(
                ActorId, OrderId, request, CancellationToken.None));

        work.VerifyNoOtherCalls();
        repo.Verify(x => x.ExpectVersion(
            It.IsAny<SalesOrder>(), It.IsAny<byte[]>()), Times.Never);
    }
}