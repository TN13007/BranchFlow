using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpBranchAvailabilityTests
{
    private readonly Mock<IMvpRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IBranchAccessService> _access = new();

    private MvpService CreateService(bool canAccessBranch = true)
    {
        _access.Setup(x => x.CanAccessBranchAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(canAccessBranch);

        _work.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new MvpService(
            _repo.Object,
            _work.Object,
            _access.Object,
            new PasswordHasher<AppUser>(),
            TimeProvider.System);
    }

    [Fact]
    public async Task SetAvailabilityAsync_CreateProductAvailability_SavesBranchProduct()
    {
        var service = CreateService();
        BranchProduct? savedRow = null;

        _repo.Setup(x => x.ProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>
            {
                new() { Id = 10, Name = "Milk Coffee" }
            });

        _repo.Setup(x => x.BranchProductsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchProduct>());

        _repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity => savedRow = entity as BranchProduct);

        var result = await service.SetAvailabilityAsync(
            100, 1, "products", 10, false, CancellationToken.None);

        Assert.Equal(10, result);
        Assert.NotNull(savedRow);
        Assert.Equal(1, savedRow.BranchId);
        Assert.Equal(10, savedRow.ProductId);
        Assert.False(savedRow.IsAvailable);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Once);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAvailabilityAsync_UpdateProductAvailability_ChangesExistingRow()
    {
        var service = CreateService();

        var existingRow = new BranchProduct
        {
            Id = 1,
            BranchId = 1,
            ProductId = 10,
            IsAvailable = true
        };

        _repo.Setup(x => x.ProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>
            {
                new() { Id = 10, Name = "Milk Coffee" }
            });

        _repo.Setup(x => x.BranchProductsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchProduct> { existingRow });

        await service.SetAvailabilityAsync(
            100, 1, "products", 10, false, CancellationToken.None);

        Assert.Equal(1, existingRow.Id);
        Assert.Equal(1, existingRow.BranchId);
        Assert.Equal(10, existingRow.ProductId);
        Assert.False(existingRow.IsAvailable);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAvailabilityAsync_CreateToppingAvailability_SavesBranchTopping()
    {
        var service = CreateService();
        BranchTopping? savedRow = null;

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping>
            {
                new() { Id = 20, Name = "Pearl", Price = 5000 }
            });

        _repo.Setup(x => x.BranchToppingsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchTopping>());

        _repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity => savedRow = entity as BranchTopping);

        var result = await service.SetAvailabilityAsync(
            100, 1, "toppings", 20, false, CancellationToken.None);

        Assert.Equal(20, result);
        Assert.NotNull(savedRow);
        Assert.Equal(1, savedRow.BranchId);
        Assert.Equal(20, savedRow.ToppingId);
        Assert.False(savedRow.IsAvailable);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Once);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAvailabilityAsync_UpdateToppingAvailability_ChangesExistingRow()
    {
        var service = CreateService();

        var existingRow = new BranchTopping
        {
            Id = 1,
            BranchId = 1,
            ToppingId = 20,
            IsAvailable = true
        };

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping>
            {
                new() { Id = 20, Name = "Pearl", Price = 5000 }
            });

        _repo.Setup(x => x.BranchToppingsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchTopping> { existingRow });

        await service.SetAvailabilityAsync(
            100, 1, "toppings", 20, false, CancellationToken.None);

        Assert.Equal(1, existingRow.Id);
        Assert.Equal(1, existingRow.BranchId);
        Assert.Equal(20, existingRow.ToppingId);
        Assert.False(existingRow.IsAvailable);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAvailabilityAsync_InaccessibleBranch_ThrowsUnauthorizedAccessException()
    {
        var service = CreateService(canAccessBranch: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.SetAvailabilityAsync(
                100, 2, "products", 10, false, CancellationToken.None));

        _repo.Verify(x => x.ProductsAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ProductNotFound_ThrowsKeyNotFoundException()
    {
        var service = CreateService();

        _repo.Setup(x => x.ProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SetAvailabilityAsync(
                100, 1, "products", 999, false, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ToppingNotFound_ThrowsKeyNotFoundException()
    {
        var service = CreateService();

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping>());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SetAvailabilityAsync(
                100, 1, "toppings", 999, false, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAvailabilityAsync_UnknownKind_ThrowsKeyNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SetAvailabilityAsync(
                100, 1, "invalid-kind", 10, false, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ProductToggle_DoesNotChangeGlobalProductPrice()
    {
        var service = CreateService();

        var product = new Product
        {
            Id = 10,
            Name = "Milk Coffee",
            ProductSizes =
            [
                new ProductSize
                {
                    Id = 100,
                    ProductId = 10,
                    SizeId = 1,
                    Price = 29000,
                    IsActive = true
                }
            ]
        };

        _repo.Setup(x => x.ProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _repo.Setup(x => x.BranchProductsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchProduct>());

        await service.SetAvailabilityAsync(
            100, 1, "products", 10, false, CancellationToken.None);

        Assert.Equal(29000, Assert.Single(product.ProductSizes).Price);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ToppingToggle_DoesNotChangeGlobalToppingPrice()
    {
        var service = CreateService();

        var topping = new Topping
        {
            Id = 20,
            Name = "Pearl",
            Price = 5000
        };

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping> { topping });

        _repo.Setup(x => x.BranchToppingsAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BranchTopping>());

        await service.SetAvailabilityAsync(
            100, 1, "toppings", 20, false, CancellationToken.None);

        Assert.Equal(5000, topping.Price);
    }
}