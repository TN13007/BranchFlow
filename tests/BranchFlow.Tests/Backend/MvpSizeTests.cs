using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpSizeTests
{
    private readonly Mock<IMvpRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IBranchAccessService> _access = new();

    private MvpService CreateService()
    {
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
    public async Task SaveOptionAsync_CreateValidSize_SavesSize()
    {
        var service = CreateService();
        Size? savedSize = null;

        _repo.Setup(x => x.Add(It.IsAny<object>()))
            .Callback<object>(entity => savedSize = entity as Size);

        var result = await service.SaveOptionAsync(
            "sizes",
            0,
            new NamedOption
            {
                Name = "XL",
                IsActive = true
            },
            CancellationToken.None);

        Assert.Equal(0, result);
        Assert.NotNull(savedSize);
        Assert.Equal("XL", savedSize.Name);
        Assert.True(savedSize.IsActive);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Once);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveOptionAsync_UpdateSize_ChangesName()
    {
        var service = CreateService();

        var existingSize = new Size
        {
            Id = 1,
            Name = "M",
            IsActive = true
        };

        _repo.Setup(x => x.SizesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Size> { existingSize });

        var result = await service.SaveOptionAsync(
            "sizes",
            1,
            new NamedOption
            {
                Name = "Medium",
                IsActive = true
            },
            CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Equal("Medium", existingSize.Name);
        Assert.True(existingSize.IsActive);

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveOptionAsync_DeactivateSize_SetsIsActiveFalse()
    {
        var service = CreateService();

        var existingSize = new Size
        {
            Id = 1,
            Name = "M",
            IsActive = true
        };

        _repo.Setup(x => x.SizesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Size> { existingSize });

        await service.SaveOptionAsync(
            "sizes",
            1,
            new NamedOption
            {
                Name = "M",
                IsActive = false
            },
            CancellationToken.None);

        Assert.False(existingSize.IsActive);
        Assert.Equal("M", existingSize.Name);

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveOptionAsync_EmptySizeName_ThrowsArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveOptionAsync(
                "sizes",
                0,
                new NamedOption
                {
                    Name = " ",
                    IsActive = true
                },
                CancellationToken.None));

        _repo.Verify(x => x.Add(It.IsAny<object>()), Times.Never);
        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveOptionAsync_UnknownKind_ThrowsKeyNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SaveOptionAsync(
                "invalid-kind",
                0,
                new NamedOption
                {
                    Name = "Test",
                    IsActive = true
                },
                CancellationToken.None));

        _work.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}