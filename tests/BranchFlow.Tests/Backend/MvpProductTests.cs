using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpProductTests
{
    private readonly Mock<IMvpRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IBranchAccessService> _access = new();

    private MvpService CreateService()
    {
        _repo.Setup(x => x.CategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>
            {
                new() { Id = 1, Name = "Coffee", IsActive = true }
            });

        _repo.Setup(x => x.SizesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Size>
            {
                new() { Id = 1, Name = "M", IsActive = true },
                new() { Id = 2, Name = "L", IsActive = true }
            });

        _work.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return new MvpService(
            _repo.Object,
            _work.Object,
            _access.Object,
            new PasswordHasher<AppUser>(),
            TimeProvider.System);
    }

    private static ProductView ValidProduct() => new()
    {
        CategoryId = 1,
        Name = "Milk Coffee",
        IsActive = true,
        Prices =
        [
            new PriceView
            {
                SizeId = 1,
                Price = 29000,
                IsActive = true
            }
        ]
    };

    [Fact]
    public async Task SaveProductAsync_CreateValidProduct_SavesProduct()
    {
        var service = CreateService();
        Product? savedProduct = null;

        _repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity => savedProduct = entity as Product);

        var result = await service.SaveProductAsync(
            0, ValidProduct(), CancellationToken.None);

        Assert.Equal(0, result);
        Assert.NotNull(savedProduct);
        Assert.Equal("Milk Coffee", savedProduct.Name);
        Assert.Equal(1, savedProduct.CategoryId);
        Assert.Single(savedProduct.ProductSizes);
        Assert.Equal(29000, savedProduct.ProductSizes.Single().Price);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveProductAsync_NegativePrice_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidProduct();
        input.Prices[0].Price = -1000;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveProductAsync(
                0, input, CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveProductAsync_PriceWithMoreThanTwoDecimalPlaces_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidProduct();
        input.Prices[0].Price = 29000.123m;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveProductAsync(
                0, input, CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveProductAsync_InvalidCategory_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidProduct();
        input.CategoryId = 999;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveProductAsync(
                0, input, CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveProductAsync_InvalidSize_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidProduct();
        input.Prices[0].SizeId = 999;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveProductAsync(
                0, input, CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveProductAsync_DuplicateSizes_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidProduct();

        input.Prices.Add(new PriceView
        {
            SizeId = 1,
            Price = 35000,
            IsActive = true
        });

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveProductAsync(
                0, input, CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveProductAsync_UpdateProduct_PreservesSizesNotIncludedInRequest()
    {
        var service = CreateService();

        var existingProduct = new Product
        {
            Id = 10,
            CategoryId = 1,
            Name = "Milk Coffee",
            IsActive = true,
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
            .ReturnsAsync(new List<Product> { existingProduct });

        var input = new ProductView
        {
            CategoryId = 1,
            Name = "Milk Coffee Updated",
            IsActive = true,
            Prices =
            [
                new PriceView
                {
                    SizeId = 2,
                    Price = 39000,
                    IsActive = true
                }
            ]
        };

        var result = await service.SaveProductAsync(
            10, input, CancellationToken.None);

        Assert.Equal(10, result);
        Assert.Equal("Milk Coffee Updated", existingProduct.Name);

        Assert.Equal(2, existingProduct.ProductSizes.Count);

        var medium = existingProduct.ProductSizes.Single(x => x.SizeId == 1);
        var large = existingProduct.ProductSizes.Single(x => x.SizeId == 2);

        Assert.Equal(29000, medium.Price);
        Assert.Equal(39000, large.Price);

        _repo.Verify(
            x => x.Add(It.IsAny<object>()),
            Times.Never);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveProductAsync_UpdateExistingSizePrice_ChangesPrice()
    {
        var service = CreateService();

        var existingProduct = new Product
        {
            Id = 10,
            CategoryId = 1,
            Name = "Milk Coffee",
            IsActive = true,
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
            .ReturnsAsync(new List<Product> { existingProduct });

        var input = new ProductView
        {
            CategoryId = 1,
            Name = "Milk Coffee",
            IsActive = true,
            Prices =
            [
                new PriceView
                {
                    SizeId = 1,
                    Price = 35000,
                    IsActive = true
                }
            ]
        };

        var result = await service.SaveProductAsync(
            10, input, CancellationToken.None);

        Assert.Equal(10, result);

        var size = Assert.Single(existingProduct.ProductSizes);
        Assert.Equal(1, size.SizeId);
        Assert.Equal(35000, size.Price);
        Assert.True(size.IsActive);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveProductAsync_DeactivateProductSize_PreservesRow()
    {
        var service = CreateService();

        var existingProduct = new Product
        {
            Id = 10,
            CategoryId = 1,
            Name = "Milk Coffee",
            IsActive = true,
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
            .ReturnsAsync(new List<Product> { existingProduct });

        var input = new ProductView
        {
            CategoryId = 1,
            Name = "Milk Coffee",
            IsActive = true,
            Prices =
            [
                new PriceView
                {
                    SizeId = 1,
                    Price = 29000,
                    IsActive = false
                }
            ]
        };

        await service.SaveProductAsync(
            10, input, CancellationToken.None);

        var size = Assert.Single(existingProduct.ProductSizes);

        Assert.Equal(100, size.Id);
        Assert.Equal(1, size.SizeId);
        Assert.Equal(29000, size.Price);
        Assert.False(size.IsActive);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

}