using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpToppingTests
{
    private readonly Mock<IMvpRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IBranchAccessService> _access = new();

    private MvpService CreateService()
    {
        _repo.Setup(x => x.ToppingGroupsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ToppingGroup>
            {
                new() { Id = 1, Name = "Extra", IsActive = true }
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

    private static ToppingView ValidTopping() => new()
    {
        GroupId = 1,
        Name = "Pearl",
        Price = 5000,
        IsActive = true
    };

    [Fact]
    public async Task SaveToppingAsync_CreateValidTopping_SavesTopping()
    {
        var service = CreateService();
        Topping? savedTopping = null;

        _repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity => savedTopping = entity as Topping);

        var result = await service.SaveToppingAsync(
            0, ValidTopping(), CancellationToken.None);

        Assert.Equal(0, result);
        Assert.NotNull(savedTopping);
        Assert.Equal(1, savedTopping.ToppingGroupId);
        Assert.Equal("Pearl", savedTopping.Name);
        Assert.Equal(5000, savedTopping.Price);
        Assert.True(savedTopping.IsActive);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Once);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveToppingAsync_UpdateTopping_ChangesNameGroupAndPrice()
    {
        var service = CreateService();

        var existingTopping = new Topping
        {
            Id = 10,
            ToppingGroupId = 1,
            Name = "Old Pearl",
            Price = 3000,
            IsActive = true
        };

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping> { existingTopping });

        var input = new ToppingView
        {
            GroupId = 1,
            Name = "Black Pearl",
            Price = 7000,
            IsActive = true
        };

        var result = await service.SaveToppingAsync(
            10, input, CancellationToken.None);

        Assert.Equal(10, result);
        Assert.Equal("Black Pearl", existingTopping.Name);
        Assert.Equal(1, existingTopping.ToppingGroupId);
        Assert.Equal(7000, existingTopping.Price);
        Assert.True(existingTopping.IsActive);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveToppingAsync_DeactivateTopping_PreservesTopping()
    {
        var service = CreateService();

        var existingTopping = new Topping
        {
            Id = 10,
            ToppingGroupId = 1,
            Name = "Pearl",
            Price = 5000,
            IsActive = true
        };

        _repo.Setup(x => x.ToppingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topping> { existingTopping });

        var input = ValidTopping();
        input.IsActive = false;

        await service.SaveToppingAsync(
            10, input, CancellationToken.None);

        Assert.Equal(10, existingTopping.Id);
        Assert.Equal("Pearl", existingTopping.Name);
        Assert.False(existingTopping.IsActive);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveToppingAsync_InvalidGroup_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidTopping();
        input.GroupId = 999;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveToppingAsync(
                0, input, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveToppingAsync_EmptyName_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidTopping();
        input.Name = " ";

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveToppingAsync(
                0, input, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveToppingAsync_NegativePrice_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidTopping();
        input.Price = -1000;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveToppingAsync(
                0, input, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveToppingAsync_PriceWithMoreThanTwoDecimalPlaces_ThrowsArgumentException()
    {
        var service = CreateService();
        var input = ValidTopping();
        input.Price = 5000.123m;

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveToppingAsync(
                0, input, CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}