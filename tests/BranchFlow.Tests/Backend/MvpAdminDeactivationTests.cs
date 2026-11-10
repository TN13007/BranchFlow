using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class MvpAdminDeactivationTests
{
    [Fact]
    public async Task UpdateUserAsync_WhenAssignedBranchAdminIsDeactivated_RejectsAndKeepsAccountActive()
    {
        var ct = CancellationToken.None;
        var account = new AppUser
        {
            Id = 42,
            Role = new Role { Code = "BRANCH_ADMIN" },
            IsActive = true,
            FullName = "Branch Manager"
        };
        var assignment = new AdminBranchAssignment
        {
            AdminUserId = account.Id,
            BranchId = 7,
            EffectiveFrom = DateTime.UtcNow.AddDays(-2)
        };
        var repo = new Mock<IMvpRepository>(MockBehavior.Strict);
        repo.Setup(x => x.UserAsync(account.Id, ct)).ReturnsAsync(account);
        repo.Setup(x => x.AdminAssignmentsAsync(ct)).ReturnsAsync([assignment]);
        var work = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var access = new Mock<IBranchAccessService>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher<AppUser>>(MockBehavior.Strict);
        var service = new MvpService(repo.Object, work.Object, access.Object, hasher.Object, TimeProvider.System);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateUserAsync(
            actor: 1,
            system: true,
            id: account.Id,
            input: new UserView { FullName = "Changed Name", IsActive = false },
            ct));

        Assert.Equal(
            "Không thể khóa Admin Chi nhánh đang được phân công. MVP chưa hỗ trợ chuyển hoặc thu hồi phân công.",
            error.Message);
        Assert.True(account.IsActive);
        Assert.Equal("Branch Manager", account.FullName);
        work.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        repo.VerifyAll();
        repo.VerifyNoOtherCalls();
        work.VerifyNoOtherCalls();
        access.VerifyNoOtherCalls();
        hasher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUserAsync_WhenBranchAdminOnlyHasExpiredAssignments_AllowsDeactivation()
    {
        var ct = CancellationToken.None;
        var account = new AppUser
        {
            Id = 43,
            Role = new Role { Code = "BRANCH_ADMIN" },
            IsActive = true,
            FullName = "Former Branch Manager"
        };
        var assignment = new AdminBranchAssignment
        {
            AdminUserId = account.Id,
            BranchId = 8,
            EffectiveFrom = DateTime.UtcNow.AddDays(-10),
            EffectiveTo = DateTime.UtcNow.AddDays(-1)
        };
        var repo = new Mock<IMvpRepository>(MockBehavior.Strict);
        repo.Setup(x => x.UserAsync(account.Id, ct)).ReturnsAsync(account);
        repo.Setup(x => x.AdminAssignmentsAsync(ct)).ReturnsAsync([assignment]);
        var work = new Mock<IUnitOfWork>(MockBehavior.Strict);
        work.Setup(x => x.SaveChangesAsync(ct)).ReturnsAsync(1);
        var access = new Mock<IBranchAccessService>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher<AppUser>>(MockBehavior.Strict);
        var service = new MvpService(repo.Object, work.Object, access.Object, hasher.Object, TimeProvider.System);

        await service.UpdateUserAsync(
            actor: 1,
            system: true,
            id: account.Id,
            input: new UserView { FullName = "Former Branch Manager", IsActive = false },
            ct);

        Assert.False(account.IsActive);
        work.VerifyAll();
        work.VerifyNoOtherCalls();
        repo.VerifyAll();
        repo.VerifyNoOtherCalls();
        access.VerifyNoOtherCalls();
        hasher.VerifyNoOtherCalls();
    }
}
