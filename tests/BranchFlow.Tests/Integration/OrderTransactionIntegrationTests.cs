using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BranchFlow.Tests.Integration;

// $env:BRANCHFLOW_TEST_CONNECTION_STRING = "Server=localhost;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True"

// try {
//     dotnet test .\tests\BranchFlow.Tests\BranchFlow.Tests.csproj -c Release --filter "FullyQualifiedName~OrderTransactionIntegrationTests"
// }
// finally {
//     Remove-Item Env:BRANCHFLOW_TEST_CONNECTION_STRING -ErrorAction SilentlyContinue
// }

[CollectionDefinition("Order SQL integration", DisableParallelization = true)]
public sealed class OrderSqlIntegrationCollection;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")))
        {
            Skip = "SQL Server integration test requires BRANCHFLOW_TEST_CONNECTION_STRING.";
        }
    }
}

[Collection("Order SQL integration")]
public sealed class OrderTransactionIntegrationTests
{
    private const int UserId = 5;
    private const int BranchId = 4;
    private const int InvalidProductSizeId = int.MaxValue;
    private const string ExpectedDatabase = "BranchFlow_Test";

    [SqlServerFact]
    public async Task CreateAsyncForeignKeyFailureRollsBackOrderAndDailyCounter()
    {
        string connectionString =
            Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set BRANCHFLOW_TEST_CONNECTION_STRING to run this integration test.");

        var connectionBuilder = new SqlConnectionStringBuilder(connectionString);

        Assert.Equal(
            ExpectedDatabase,
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<BranchFlowDbContext>(
            options => options.UseSqlServer(connectionString));

        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IBranchAccessService, BranchAccessService>();

        services.AddScoped<IOrderMenuRepository, OrderMenuRepository>();
        services.AddScoped<IOrderMenuService, OrderMenuService>();

        services.AddScoped<OrderRepository>();
        services.AddScoped<IOrderRepository>(provider =>
            new InvalidProductSizeOrderRepository(
                provider.GetRequiredService<OrderRepository>()));

        services.AddScoped<IOrderService, OrderService>();

        await using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true });

        var now = TimeProvider.System.GetUtcNow();
        var businessDate = DateOnly.FromDateTime(
            now.ToOffset(TimeSpan.FromHours(7)).DateTime);

        int productSizeId;

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<BranchFlowDbContext>();

            Assert.False(
                await db.ProductSizes.AnyAsync(
                    x => x.Id == InvalidProductSizeId),
                "InvalidProductSizeId unexpectedly exists.");

            var menuService = scope.ServiceProvider
                .GetRequiredService<IOrderMenuService>();

            var menu = await menuService.GetMenuByBranchAsync(
                UserId,
                BranchId);

            Assert.NotNull(menu);

            productSizeId = menu.Categories
                .SelectMany(category => category.Products)
                .SelectMany(product => product.ProductSizes)
                .Select(size => size.Id)
                .First();

            Assert.True(productSizeId > 0);
        }

        var before = await ReadSnapshotAsync(
            provider,
            businessDate);

        await using (var scope = provider.CreateAsyncScope())
        {
            var service = scope.ServiceProvider
                .GetRequiredService<IOrderService>();

            var request = new CreateOrderRequest(
                BranchId,
                [
                    new CreateOrderItem(
                        productSizeId,
                        1,
                        [],
                        [])
                ]);

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => service.CreateAsync(UserId, request));

            var sqlException = exception.GetBaseException() as SqlException;

            Assert.NotNull(sqlException);
            Assert.Equal(547, sqlException.Number);
        }

        var after = await ReadSnapshotAsync(
            provider,
            businessDate);

        Assert.Equal(before.SalesOrderCount, after.SalesOrderCount);
        Assert.Equal(before.OrderItemCount, after.OrderItemCount);
        Assert.Equal(before.CounterLastNumber, after.CounterLastNumber);
    }

    private static async Task<DatabaseSnapshot> ReadSnapshotAsync(
        ServiceProvider provider,
        DateOnly businessDate)
    {
        await using var scope = provider.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<BranchFlowDbContext>();

        int salesOrderCount = await db.SalesOrders
            .CountAsync(order =>
                order.BranchId == BranchId &&
                order.BusinessDate == businessDate);

        int orderItemCount = await db.OrderItems
            .CountAsync(item =>
                item.SalesOrder.BranchId == BranchId &&
                item.SalesOrder.BusinessDate == businessDate);

        int? counterLastNumber = await db.OrderDailyCounters
            .Where(counter =>
                counter.BranchId == BranchId &&
                counter.BusinessDate == businessDate)
            .Select(counter => (int?)counter.LastNumber)
            .SingleOrDefaultAsync();

        return new DatabaseSnapshot(
            salesOrderCount,
            orderItemCount,
            counterLastNumber);
    }

    private sealed record DatabaseSnapshot(
        int SalesOrderCount,
        int OrderItemCount,
        int? CounterLastNumber);

    private sealed class InvalidProductSizeOrderRepository(
        IOrderRepository inner) : IOrderRepository
    {
        public Task<(string Code, int Sequence)> AllocateCodeAsync(
            int branchId,
            DateOnly date,
            CancellationToken ct)
            => inner.AllocateCodeAsync(branchId, date, ct);

        public void Add(SalesOrder order)
        {
            var firstItem = order.OrderItems.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "The test order has no order items.");

            firstItem.ProductSizeId = InvalidProductSizeId;

            inner.Add(order);
        }

        public Task<List<SalesOrder>> GetMineAsync(
            int userId,
            IReadOnlyList<int> branchIds,
            string? keyword,
            DateOnly? date,
            CancellationToken ct)
            => inner.GetMineAsync(
                userId,
                branchIds,
                keyword,
                date,
                ct);

        public Task<SalesOrder?> GetDetailAsync(
            int id,
            int userId,
            CancellationToken ct)
            => inner.GetDetailAsync(id, userId, ct);
    }
}