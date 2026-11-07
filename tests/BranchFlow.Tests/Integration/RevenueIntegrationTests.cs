using System.Data;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Tests.Integration;

[CollectionDefinition("Revenue SQL integration", DisableParallelization = true)]
public sealed class RevenueSqlIntegrationCollection;

[Collection("Revenue SQL integration")]
public sealed class RevenueIntegrationTests
{
    private const string ExpectedDatabase = "BranchFlow_Test";
    private const int BranchId = 4;
    private const int CreatedByUserId = 8;

    [SqlServerFact]
    public async Task RevenueAsync_UsesVietnamDateAndCurrentCompletedStatus()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set BRANCHFLOW_TEST_CONNECTION_STRING.");

        var connectionBuilder =
            new SqlConnectionStringBuilder(connectionString);

        Assert.Equal(
            ExpectedDatabase,
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var options = new DbContextOptionsBuilder<BranchFlowDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new BranchFlowDbContext(options);

        await using var transaction =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            Assert.True(await db.Branches.AnyAsync(x => x.Id == BranchId && !x.Deleted));
            Assert.True(await db.AppUsers.AnyAsync(x => x.Id == CreatedByUserId && !x.Deleted));

            var november6 = new DateOnly(2026, 11, 6);
            var november7 = new DateOnly(2026, 11, 7);

            var sequence6 = await db.SalesOrders
                .Where(x => x.BranchId == BranchId && x.BusinessDate == november6)
                .MaxAsync(x => (int?)x.DailySequence) ?? 0;

            var sequence7 = await db.SalesOrders
                .Where(x => x.BranchId == BranchId && x.BusinessDate == november7)
                .MaxAsync(x => (int?)x.DailySequence) ?? 0;

            var beforeMidnight = new SalesOrder
            {
                Code = $"REV-{Guid.NewGuid():N}",
                BranchId = BranchId,
                CreatedByUserId = CreatedByUserId,
                BusinessDate = november6,
                DailySequence = ++sequence6,
                TotalAmount = 100m,
                Status = "COMPLETED",
                CreatedAt = new DateTime(2026, 11, 6, 16, 59, 0, DateTimeKind.Utc)
            };

            var atMidnight = new SalesOrder
            {
                Code = $"REV-{Guid.NewGuid():N}",
                BranchId = BranchId,
                CreatedByUserId = CreatedByUserId,
                BusinessDate = november7,
                DailySequence = ++sequence7,
                TotalAmount = 200m,
                Status = "COMPLETED",
                CreatedAt = new DateTime(2026, 11, 6, 17, 0, 0, DateTimeKind.Utc)
            };

            var cancelled = new SalesOrder
            {
                Code = $"REV-{Guid.NewGuid():N}",
                BranchId = BranchId,
                CreatedByUserId = CreatedByUserId,
                BusinessDate = november7,
                DailySequence = ++sequence7,
                TotalAmount = 300m,
                Status = "CANCELLED",
                CreatedAt = new DateTime(2026, 11, 6, 18, 0, 0, DateTimeKind.Utc)
            };

            var completedLater = new SalesOrder
            {
                Code = $"REV-{Guid.NewGuid():N}",
                BranchId = BranchId,
                CreatedByUserId = CreatedByUserId,
                BusinessDate = november7,
                DailySequence = ++sequence7,
                TotalAmount = 400m,
                Status = "COMPLETED",
                CreatedAt = new DateTime(2026, 11, 7, 2, 0, 0, DateTimeKind.Utc)
            };

            db.SalesOrders.AddRange(
                beforeMidnight,
                atMidnight,
                cancelled,
                completedLater);

            await db.SaveChangesAsync();

            var repository = new MvpRepository(db);

            var initialResult = await repository.RevenueAsync(
                [BranchId],
                new DateTime(2026, 11, 5, 17, 0, 0),
                new DateTime(2026, 11, 7, 17, 0, 0),
                CancellationToken.None);

            Assert.Equal(2, initialResult.Count);

            var resultNovember6 = Assert.Single(initialResult,
    x => x.VietnamDate.Date == november6.ToDateTime(TimeOnly.MinValue));

            Assert.Equal(1, resultNovember6.Orders);
            Assert.Equal(100m, resultNovember6.Amount);

            var resultNovember7 = Assert.Single(initialResult,
    x => x.VietnamDate.Date == november7.ToDateTime(TimeOnly.MinValue));

            Assert.Equal(2, resultNovember7.Orders);
            Assert.Equal(600m, resultNovember7.Amount);

            completedLater.Status = "CANCELLED";
            completedLater.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var updatedResult = await repository.RevenueAsync(
                [BranchId],
                new DateTime(2026, 11, 5, 17, 0, 0),
                new DateTime(2026, 11, 7, 17, 0, 0),
                CancellationToken.None);

            Assert.Equal(2, updatedResult.Count);

            resultNovember6 = Assert.Single(updatedResult,
    x => x.VietnamDate.Date == november6.ToDateTime(TimeOnly.MinValue));

            Assert.Equal(1, resultNovember6.Orders);
            Assert.Equal(100m, resultNovember6.Amount);

            resultNovember7 = Assert.Single(updatedResult,
    x => x.VietnamDate.Date == november7.ToDateTime(TimeOnly.MinValue));

            Assert.Equal(1, resultNovember7.Orders);
            Assert.Equal(200m, resultNovember7.Amount);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }
}