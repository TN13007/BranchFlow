using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpRevenueValidationTests
{
    private readonly Mock<IMvpRepository> repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> work = new();
    private readonly Mock<IBranchAccessService> access = new(MockBehavior.Strict);
    private readonly Mock<IPasswordHasher<AppUser>> hasher = new();

    private MvpService CreateService() => new(
        repo.Object,
        work.Object,
        access.Object,
        hasher.Object,
        TimeProvider.System);

    [Fact]
    public async Task RevenueAsync_WhenFromIsAfterTo_ThrowsArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RevenueAsync(1, null, new DateOnly(2026, 11, 8),
                new DateOnly(2026, 11, 7), CancellationToken.None));

        access.VerifyNoOtherCalls();
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevenueAsync_WhenRangeIs91Days_AcceptsRangeAndUsesVietnamUtcBoundaries()
    {
        var ct = CancellationToken.None;
        var service = CreateService();

        access.Setup(x => x.GetAccessibleBranchesAsync(1, ct))
            .ReturnsAsync(new List<AccessibleBranch>
            {
                new(4, "BF-04", "Branch 04")
            });

        repo.Setup(x => x.RevenueAsync(
                It.Is<int[]>(ids => ids.SequenceEqual(new[] { 4 })),
                new DateTime(2025, 12, 31, 17, 0, 0),
                new DateTime(2026, 4, 1, 17, 0, 0),
                ct))
            .ReturnsAsync(new List<RevenueData>());

        var result = await service.RevenueAsync(
            1, null,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 1),
            ct);

        Assert.Empty(result);
        access.VerifyAll();
        repo.VerifyAll();
    }

    [Fact]
    public async Task RevenueAsync_WhenRangeIs92Days_ThrowsArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RevenueAsync(1, null, new DateOnly(2026, 1, 1),
                new DateOnly(2026, 4, 2), CancellationToken.None));

        access.VerifyNoOtherCalls();
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevenueAsync_WhenFromYearIsBefore2000_ThrowsArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RevenueAsync(1, null, new DateOnly(1999, 12, 31),
                new DateOnly(2000, 1, 1), CancellationToken.None));

        access.VerifyNoOtherCalls();
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevenueAsync_WhenToYearIsAfter2100_ThrowsArgumentException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RevenueAsync(1, null, new DateOnly(2100, 12, 31),
                new DateOnly(2101, 1, 1), CancellationToken.None));

        access.VerifyNoOtherCalls();
        repo.VerifyNoOtherCalls();
    }
}