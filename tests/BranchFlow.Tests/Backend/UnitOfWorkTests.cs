using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_UsesInjectedDbContext_AndReturnsAffectedCount()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        DbContextOptions<BranchFlowDbContext> options =
            new DbContextOptionsBuilder<BranchFlowDbContext>().Options;

        var context = new Mock<BranchFlowDbContext>(options);
        context
            .Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(3);

        var unitOfWork = new UnitOfWork(context.Object);

        int affectedEntries = await unitOfWork.SaveChangesAsync(cancellationToken);

        Assert.Equal(3, affectedEntries);
        context.Verify(
            db => db.SaveChangesAsync(cancellationToken),
            Times.Once);
    }
}
