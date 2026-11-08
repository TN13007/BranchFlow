using System.Data;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Tests.Integration;

[Collection("Revenue SQL integration")]
public sealed class RevenueServiceIntegrationTests
{
    private const string ExpectedDatabase = "BranchFlow_Test";
    private int BranchId;
    private int CreatedByUserId;

    private static readonly DateOnly From = new(2026, 11, 6);
    private static readonly DateOnly To = new(2026, 11, 7);
    private static readonly DateOnly OrderDate = new(2026, 11, 7);

    [SqlServerFact]
    public async Task RevenueAsync_ReturnsOnlyBranchesAccessibleToCurrentUser()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Set BRANCHFLOW_TEST_CONNECTION_STRING.");

        SqlConnectionStringBuilder builder = new(connectionString);
        Assert.Equal(ExpectedDatabase, builder.InitialCatalog, ignoreCase: true);

        DbContextOptions<BranchFlowDbContext> options =
            new DbContextOptionsBuilder<BranchFlowDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        await using BranchFlowDbContext db = new(options);
        BranchId = await db.Branches.Where(x => x.Code == "BF-DEMO-01").Select(x => x.Id).SingleAsync();
        CreatedByUserId = await db.AppUsers.Where(x => x.Username == "demo.employee.a").Select(x => x.Id).SingleAsync();
        int ownerId = await db.AppUsers.Where(x => x.Username == "demo.owner").Select(x => x.Id).SingleAsync();
        int systemAdminId = await db.AppUsers.Where(x => x.Username == "demo.systemadmin").Select(x => x.Id).SingleAsync();
        int branchAdminId = await db.AppUsers.Where(x => x.Username == "demo.branchadmin.01").Select(x => x.Id).SingleAsync();
        int otherBranchId = await db.Branches.Where(x => x.Code == "BF-DEMO-02").Select(x => x.Id).SingleAsync();

        TimeProvider clock = new FixedTimeProvider(
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        AuthRepository authRepository = new(db, clock);
        BranchAccessService accessService = new(authRepository);
        MvpRepository repository = new(db);
        UnitOfWork unitOfWork = new(db);

        MvpService service = new(
            repository,
            unitOfWork,
            accessService,
            new PasswordHasher<AppUser>(),
            clock);

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            // Kiểm tra fixture trước khi tạo dữ liệu test.
            Assert.True(await db.Branches.AnyAsync(x => x.Id == BranchId));
            Assert.True(await db.AppUsers.AnyAsync(x => x.Id == CreatedByUserId));

            // Lưu kết quả ban đầu để kiểm tra phần chênh lệch sau khi thêm đơn hàng.
            var baseline = await service.RevenueAsync(
                ownerId, null, From, To, CancellationToken.None);

            var baselineBranch = baseline.SingleOrDefault(
                x => x.BranchId == BranchId && x.Date == OrderDate);

            int baselineOrders = baselineBranch?.Orders ?? 0;
            decimal baselineAmount = baselineBranch?.Amount ?? 0m;

            // Tạo đơn hàng COMPLETED thuộc Branch 4.
            int dailySequence = await db.SalesOrders
                .Where(x => x.BranchId == BranchId && x.BusinessDate == OrderDate)
                .Select(x => (int?)x.DailySequence)
                .MaxAsync() ?? 0;

            db.SalesOrders.Add(new SalesOrder
            {
                Code = $"REV-{Guid.NewGuid():N}",
                BranchId = BranchId,
                CreatedByUserId = CreatedByUserId,
                BusinessDate = OrderDate,
                DailySequence = dailySequence + 1,
                TotalAmount = 123m,
                Status = "COMPLETED",
                CreatedAt = new DateTime(2026, 11, 6, 17, 0, 0, DateTimeKind.Utc)
            });

            await db.SaveChangesAsync();

            // Owner: kết quả phải bao gồm đơn hàng vừa tạo.
            var ownerResult = await service.RevenueAsync(
                ownerId, null, From, To, CancellationToken.None);

            var ownerBranch = Assert.Single(
                ownerResult,
                x => x.BranchId == BranchId && x.Date == OrderDate);

            Assert.Equal(baselineOrders + 1, ownerBranch.Orders);
            // Tổng doanh thu toàn hệ thống phải bằng tổng doanh thu từng chi nhánh.
            int systemOrders = ownerResult.Sum(x => x.Orders);
            decimal systemAmount = ownerResult.Sum(x => x.Amount);

            int branchOrders = 0;
            decimal branchAmount = 0m;

            foreach (int branchId in ownerResult.Select(x => x.BranchId).Distinct())
            {
                var branchResult = await service.RevenueAsync(
                    ownerId, branchId, From, To, CancellationToken.None);

                branchOrders += branchResult.Sum(x => x.Orders);
                branchAmount += branchResult.Sum(x => x.Amount);
            }

            Assert.Equal(systemOrders, branchOrders);
            Assert.Equal(systemAmount, branchAmount);

            // System Admin: phạm vi kết quả phải giống Owner.
            var systemAdminResult = await service.RevenueAsync(
                systemAdminId, null, From, To, CancellationToken.None);

            Assert.Equal(
                ownerResult
                    .OrderBy(x => x.BranchId)
                    .ThenBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)),
                systemAdminResult
                    .OrderBy(x => x.BranchId)
                    .ThenBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)));

            // Branch Admin 01: chỉ được xem Branch 4.
            var branchAdminResult = await service.RevenueAsync(
                branchAdminId, null, From, To, CancellationToken.None);

            Assert.NotEmpty(branchAdminResult);
            Assert.All(branchAdminResult, x => Assert.Equal(BranchId, x.BranchId));

            // Kết quả Branch Admin phải khớp phần Branch 4 của Owner.
            Assert.Equal(
                ownerResult
                    .Where(x => x.BranchId == BranchId)
                    .OrderBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)),
                branchAdminResult
                    .OrderBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)));

            // Branch Admin 01 không được truy vấn Branch 5.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.RevenueAsync(branchAdminId, otherBranchId, From, To, CancellationToken.None));

            // Branch Admin 01 được chỉ định rõ Branch 4.
            var selectedBranchResult = await service.RevenueAsync(
                branchAdminId, BranchId, From, To, CancellationToken.None);

            Assert.Equal(
                branchAdminResult
                    .OrderBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)),
                selectedBranchResult
                    .OrderBy(x => x.Date)
                    .Select(x => (x.BranchId, x.Date, x.Orders, x.Amount)));
        }
        finally
        {
            // Không để lại dữ liệu test trong BranchFlow_Test.
            await transaction.RollbackAsync();
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
