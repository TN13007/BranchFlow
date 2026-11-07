using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Tests.Integration;

public sealed class AuthRepositoryAccessIntegrationTests
{
    private const string ExpectedDatabase = "BranchFlow_Test";

    [SqlServerFact]
    public async Task GetAccessibleBranchesAsync_ReturnsBranchesAccordingToUserRoleAndAssignments()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set BRANCHFLOW_TEST_CONNECTION_STRING.");

        SqlConnectionStringBuilder connectionBuilder =
            new(connectionString);

        Assert.Equal(
            ExpectedDatabase,
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        DbContextOptions<BranchFlowDbContext> options =
            new DbContextOptionsBuilder<BranchFlowDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        await using BranchFlowDbContext db = new(options);

        TimeProvider clock = new FixedTimeProvider(
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        AuthRepository repository = new(db, clock);

        // Lấy danh sách toàn bộ chi nhánh đang hoạt động để đối chiếu Owner/System Admin.
        int[] allActiveBranchIds = await db.Branches
            .AsNoTracking()
            .Where(branch => branch.IsActive && !branch.Deleted)
            .OrderBy(branch => branch.Id)
            .Select(branch => branch.Id)
            .ToArrayAsync();

        Assert.NotEmpty(allActiveBranchIds);

        // Owner và System Admin được truy cập toàn bộ chi nhánh đang hoạt động.
        IReadOnlyList<AccessibleBranchData> ownerBranches =
            await repository.GetAccessibleBranchesAsync(3);

        IReadOnlyList<AccessibleBranchData> systemAdminBranches =
            await repository.GetAccessibleBranchesAsync(4);

        Assert.Equal(
            allActiveBranchIds,
            ownerBranches.Select(branch => branch.Id).OrderBy(id => id).ToArray());

        Assert.Equal(
            allActiveBranchIds,
            systemAdminBranches.Select(branch => branch.Id).OrderBy(id => id).ToArray());

        // Mỗi Branch Admin chỉ được truy cập chi nhánh được phân công.
        await AssertAccessibleBranchesAsync(repository, 5, 4);
        await AssertAccessibleBranchesAsync(repository, 6, 5);
        await AssertAccessibleBranchesAsync(repository, 7, 6);

        // Employee chỉ được truy cập chi nhánh có assignment còn hiệu lực.
        await AssertAccessibleBranchesAsync(repository, 8, 4);
        await AssertAccessibleBranchesAsync(repository, 9, 5);

        // Tài khoản Employee bị khóa không được truy cập chi nhánh.
        await AssertNoAccessibleBranchesAsync(repository, 10);

        // Employee đang hoạt động nhưng chưa được phân công không có chi nhánh.
        await AssertNoAccessibleBranchesAsync(repository, 11);

        // User ID không hợp lệ không được truy cập chi nhánh.
        await AssertNoAccessibleBranchesAsync(repository, 0);
        await AssertNoAccessibleBranchesAsync(repository, -1);
    }

    private static async Task AssertAccessibleBranchesAsync(
        AuthRepository repository,
        int userId,
        int expectedBranchId)
    {
        IReadOnlyList<AccessibleBranchData> branches =
            await repository.GetAccessibleBranchesAsync(userId);

        AccessibleBranchData branch = Assert.Single(branches);

        Assert.Equal(expectedBranchId, branch.Id);
    }

    private static async Task AssertNoAccessibleBranchesAsync(
        AuthRepository repository,
        int userId)
    {
        IReadOnlyList<AccessibleBranchData> branches =
            await repository.GetAccessibleBranchesAsync(userId);

        Assert.Empty(branches);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}