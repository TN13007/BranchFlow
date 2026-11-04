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
    public async Task ResetDemoPasswordsUpdatesOnlyTheKnownNineDemoAccounts()
    {
        PasswordHasher<AppUser> passwordHasher = new();
        List<AppUser> users =
        [
            CreateUser(1, "demo.owner", "OWNER"),
            CreateUser(2, "demo.systemadmin", "SYSTEM_ADMIN"),
            CreateUser(3, "demo.branchadmin.01", "BRANCH_ADMIN"),
            CreateUser(4, "demo.branchadmin.02", "BRANCH_ADMIN"),
            CreateUser(5, "demo.branchadmin.03", "BRANCH_ADMIN"),
            CreateUser(6, "demo.employee.a", "EMPLOYEE"),
            CreateUser(7, "demo.employee.b", "EMPLOYEE"),
            CreateUser(8, "demo.employee.locked", "EMPLOYEE"),
            CreateUser(9, "demo.employee.unassigned", "EMPLOYEE")
        ];

        Mock<IDemoSeedRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetUsersByUsernamesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Count);
        unitOfWork
            .Setup(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DemoSeedService service = new(
            repository.Object,
            unitOfWork.Object,
            passwordHasher,
            TimeProvider.System);

        const string newPassword = "BranchFlow-Demo-Reset-2026!";
        int updatedCount = await service.ResetDemoPasswordsAsync(newPassword);

        Assert.Equal(9, updatedCount);
        foreach (AppUser user in users)
        {
            Assert.Equal(
                PasswordVerificationResult.Success,
                passwordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword));
        }

        unitOfWork.Verify(
            unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            unit => unit.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AppUser CreateUser(int id, string username, string roleCode)
    {
        return new AppUser
        {
            Id = id,
            Username = username,
            PasswordHash = "old-hash",
            FullName = username,
            IsActive = !username.EndsWith(".locked", StringComparison.Ordinal),
            CreatedAt = DateTime.UtcNow,
            RoleId = id,
            Role = new Role
            {
                Id = id,
                Code = roleCode,
                Name = roleCode,
                CreatedAt = DateTime.UtcNow
            }
        };
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
