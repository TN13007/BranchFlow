using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class DemoSeedServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("            ")]
    public async Task SeedRejectsInvalidPasswordBeforeStartingTransaction(string password)
    {
        var repository = new Mock<IDemoSeedRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var service = new DemoSeedService(
            repository.Object,
            unitOfWork.Object,
            new PasswordHasher<AppUser>(),
            TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SeedAsync(password));

        unitOfWork.Verify(
            unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SeedRollsBackWhenRequiredRoleIsMissing()
    {
        var repository = new Mock<IDemoSeedRepository>(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetRoleByCodeAsync("OWNER", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork
            .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(unit => unit.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new DemoSeedService(
            repository.Object,
            unitOfWork.Object,
            new PasswordHasher<AppUser>(),
            TimeProvider.System);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SeedAsync("Demo-Password-2026!"));

        unitOfWork.Verify(
            unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        unitOfWork.Verify(
            unit => unit.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
