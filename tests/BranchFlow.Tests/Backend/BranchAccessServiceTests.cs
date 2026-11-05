using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Repositories;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class BranchAccessServiceTests
{
    [Fact]
    public async Task GetAccessibleBranchesMapsRepositoryDataToApiDto()
    {
        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetAccessibleBranchesAsync(17, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccessibleBranchData>
            {
                new(1, "BF-DEMO-01", "Chi nhánh 01"),
                new(3, "BF-DEMO-03", "Chi nhánh 03")
            });

        IBranchAccessService service = new BranchAccessService(repository.Object);

        IReadOnlyList<AccessibleBranch> result =
            await service.GetAccessibleBranchesAsync(17);

        Assert.Equal(2, result.Count);
        Assert.Equal(new AccessibleBranch(1, "BF-DEMO-01", "Chi nhánh 01"), result[0]);
        Assert.Equal(new AccessibleBranch(3, "BF-DEMO-03", "Chi nhánh 03"), result[1]);
        repository.VerifyAll();
    }

    [Fact]
    public async Task CanAccessBranchReturnsTrueOnlyWhenBranchIsInCurrentScope()
    {
        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetAccessibleBranchesAsync(17, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccessibleBranchData>
            {
                new(1, "BF-DEMO-01", "Chi nhánh 01")
            });

        IBranchAccessService service = new BranchAccessService(repository.Object);

        bool canAccessAssignedBranch = await service.CanAccessBranchAsync(17, 1);
        bool canAccessOtherBranch = await service.CanAccessBranchAsync(17, 2);

        Assert.True(canAccessAssignedBranch);
        Assert.False(canAccessOtherBranch);
        repository.Verify(repo => repo.GetAccessibleBranchesAsync(17, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(17, 0)]
    [InlineData(17, -1)]
    public async Task InvalidUserOrBranchCannotAccessBranch(int userId, int branchId)
    {
        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        IBranchAccessService service = new BranchAccessService(repository.Object);

        bool result = await service.CanAccessBranchAsync(userId, branchId);

        Assert.False(result);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UserWithoutAssignmentsReceivesEmptyBranchList()
    {
        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetAccessibleBranchesAsync(22, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AccessibleBranchData>());

        IBranchAccessService service = new BranchAccessService(repository.Object);

        IReadOnlyList<AccessibleBranch> result =
            await service.GetAccessibleBranchesAsync(22);

        Assert.Empty(result);
        repository.VerifyAll();
    }
}
