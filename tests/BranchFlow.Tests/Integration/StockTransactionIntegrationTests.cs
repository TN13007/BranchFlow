using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BranchFlow.Tests.Integration;

[CollectionDefinition("Stock SQL integration", DisableParallelization = true)]
public sealed class StockSqlIntegrationCollection;

[Collection("Stock SQL integration")]
public sealed class StockTransactionIntegrationTests
{
    private const string ExpectedDatabase = "BranchFlow_Test";
    private int BranchId;
    private int SystemAdminId;
    private int BranchAdminId;
    private int OtherBranchId;
    private const int UnitId = 1;

    [SqlServerFact]
    public async Task WriteStockAsync_ValidatesStockQuantityRowVersionAndConcurrency()
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

        int ingredientId = 0;

        try
        {
            // Kiểm tra fixture và tạo Ingredient riêng cho test.
            await using (var db = new BranchFlowDbContext(options))
            {
                BranchId = await db.Branches.Where(x => x.Code == "BF-DEMO-01").Select(x => x.Id).SingleAsync();
                OtherBranchId = await db.Branches.Where(x => x.Code == "BF-DEMO-02").Select(x => x.Id).SingleAsync();
                SystemAdminId = await db.AppUsers.Where(x => x.Username == "demo.systemadmin").Select(x => x.Id).SingleAsync();
                BranchAdminId = await db.AppUsers.Where(x => x.Username == "demo.branchadmin.01").Select(x => x.Id).SingleAsync();
                Assert.True(await db.Branches.AnyAsync(x => x.Id == BranchId && !x.Deleted));
                Assert.True(await db.Branches.AnyAsync(x => x.Id == OtherBranchId && !x.Deleted));
                Assert.True(await db.AppUsers.AnyAsync(x => x.Id == SystemAdminId && !x.Deleted && x.IsActive));
                Assert.True(await db.AppUsers.AnyAsync(x => x.Id == BranchAdminId && !x.Deleted && x.IsActive));
                Assert.True(await db.IngredientUnits.AnyAsync(x => x.Id == UnitId && x.IsActive));

                string ingredientName = $"__TEST_STOCK_{Guid.NewGuid():N}";

                var ingredient = new Ingredient
                {
                    UnitId = UnitId,
                    Name = ingredientName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Deleted = false
                };

                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();

                ingredientId = ingredient.Id;
            }

            // Nhập 10.
            await WriteStockAsync(options, SystemAdminId, new StockWriteRequest
            {
                BranchId = BranchId,
                Type = "IN",
                Reason = "Integration test - stock in",
                Lines =
                [
                    new StockLineRequest
                    {
                        IngredientId = ingredientId,
                        Quantity = 10,
                        RowVersion = ""
                    }
                ]
            });

            var afterImport = await ReadBalanceAsync(options, ingredientId);
            Assert.Equal(10m, afterImport.Quantity);
            Assert.False(string.IsNullOrWhiteSpace(afterImport.RowVersion));

            // Xuất 3, tồn còn 7.
            await WriteStockAsync(options, SystemAdminId, new StockWriteRequest
            {
                BranchId = BranchId,
                Type = "OUT",
                Reason = "Integration test - stock out",
                Lines =
                [
                    new StockLineRequest
                    {
                        IngredientId = ingredientId,
                        Quantity = 3,
                        RowVersion = afterImport.RowVersion
                    }
                ]
            });

            var afterExport = await ReadBalanceAsync(options, ingredientId);
            Assert.Equal(7m, afterExport.Quantity);

            // Xuất quá tồn phải bị chặn.
            int transactionCountBeforeOverdraw =
                await CountStockTransactionsAsync(options, ingredientId);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                WriteStockAsync(options, SystemAdminId, new StockWriteRequest
                {
                    BranchId = BranchId,
                    Type = "OUT",
                    Reason = "Integration test - overdraw",
                    Lines =
                    [
                        new StockLineRequest
                        {
                            IngredientId = ingredientId,
                            Quantity = 8,
                            RowVersion = afterExport.RowVersion
                        }
                    ]
                }));

            var afterOverdraw = await ReadBalanceAsync(options, ingredientId);
            int transactionCountAfterOverdraw =
                await CountStockTransactionsAsync(options, ingredientId);

            Assert.Equal(7m, afterOverdraw.Quantity);
            Assert.Equal(transactionCountBeforeOverdraw, transactionCountAfterOverdraw);

            // RowVersion cũ phải bị từ chối sau khi tồn kho thay đổi.
            string staleRowVersion = afterOverdraw.RowVersion;

            await WriteStockAsync(options, SystemAdminId, new StockWriteRequest
            {
                BranchId = BranchId,
                Type = "OUT",
                Reason = "Integration test - update row version",
                Lines =
                [
                    new StockLineRequest
                    {
                        IngredientId = ingredientId,
                        Quantity = 1,
                        RowVersion = staleRowVersion
                    }
                ]
            });

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                WriteStockAsync(options, SystemAdminId, new StockWriteRequest
                {
                    BranchId = BranchId,
                    Type = "OUT",
                    Reason = "Integration test - stale row version",
                    Lines =
                    [
                        new StockLineRequest
                        {
                            IngredientId = ingredientId,
                            Quantity = 1,
                            RowVersion = staleRowVersion
                        }
                    ]
                }));

            var afterStaleRequest = await ReadBalanceAsync(options, ingredientId);
            Assert.Equal(6m, afterStaleRequest.Quantity);

            // Hai request đồng thời cùng xuất 4 từ tồn kho 6.
            // Cả hai cùng gửi RowVersion hiện tại.
            int transactionCountBeforeConcurrency =
                await CountStockTransactionsAsync(options, ingredientId);

            var request1 = CreateStockRequest(
                ingredientId,
                4,
                afterStaleRequest.RowVersion,
                "Integration test - concurrent export 1");

            var request2 = CreateStockRequest(
                ingredientId,
                4,
                afterStaleRequest.RowVersion,
                "Integration test - concurrent export 2");

            bool[] results = await Task.WhenAll(
                TryWriteStockConcurrentlyAsync(options, SystemAdminId, request1),
                TryWriteStockConcurrentlyAsync(options, SystemAdminId, request2));

            // Chỉ một request được phép thành công.
            Assert.Single(results, x => x);

            var afterConcurrency = await ReadBalanceAsync(options, ingredientId);
            int transactionCountAfterConcurrency =
                await CountStockTransactionsAsync(options, ingredientId);

            Assert.Equal(2m, afterConcurrency.Quantity);
            Assert.True(afterConcurrency.Quantity >= 0m);
            Assert.Equal(transactionCountBeforeConcurrency + 1, transactionCountAfterConcurrency);

            // Branch Admin 01 không được truy cập Branch 5.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                WriteStockAsync(options, BranchAdminId, new StockWriteRequest
                {
                    BranchId = OtherBranchId,
                    Type = "IN",
                    Reason = "Integration test - cross branch",
                    Lines =
                    [
                        new StockLineRequest
                        {
                            IngredientId = ingredientId,
                            Quantity = 1,
                            RowVersion = ""
                        }
                    ]
                }, employee: false));

            // Đối chiếu lịch sử: IN 10, OUT 3, OUT 1, OUT 4.
            await using (var db = new BranchFlowDbContext(options))
            {
                var details = await db.StockTransactionDetails
                                    .Where(x => x.IngredientId == ingredientId)
                                    .OrderBy(x => x.Id)
                                    .Select(x => new
                                    {
                                        x.StockTransactionId,
                                        x.QuantityBefore,
                                        x.QuantityChange,
                                        x.QuantityAfter
                                    })
                                    .ToListAsync();

                Assert.Equal(4, details.Count);

                Assert.Equal((0m, 10m, 10m),
                    (details[0].QuantityBefore, details[0].QuantityChange, details[0].QuantityAfter));

                Assert.Equal((10m, -3m, 7m),
                    (details[1].QuantityBefore, details[1].QuantityChange, details[1].QuantityAfter));

                Assert.Equal((7m, -1m, 6m),
                    (details[2].QuantityBefore, details[2].QuantityChange, details[2].QuantityAfter));

                Assert.Equal((6m, -4m, 2m),
                    (details[3].QuantityBefore, details[3].QuantityChange, details[3].QuantityAfter));

                // Kiểm tra AuditLog của các giao dịch thành công.
                var transactionIds = details.Select(x => x.StockTransactionId).ToList();

                var auditLogs = await db.AuditLogs
                    .Where(x => x.EntityName == "StockTransaction" &&
                                transactionIds.Contains(x.EntityId))
                    .ToListAsync();

                Assert.Equal(4, auditLogs.Count);

                Assert.All(auditLogs, audit =>
                {
                    Assert.Equal(BranchId, audit.BranchId);
                    Assert.Equal(SystemAdminId, audit.PerformedByUserId);
                    Assert.Equal("CREATE", audit.Action);
                    Assert.False(string.IsNullOrWhiteSpace(audit.AfterData));
                    Assert.Contains("Reason", audit.AfterData);
                });
            }
        }
        finally
        {
            // Chỉ xóa dữ liệu được tạo bởi test này.
            if (ingredientId > 0)
            {
                await CleanupAsync(options, ingredientId);
            }
        }
    }

    private StockWriteRequest CreateStockRequest(
        int ingredientId,
        decimal quantity,
        string rowVersion,
        string reason)
    {
        return new StockWriteRequest
        {
            BranchId = BranchId,
            Type = "OUT",
            Reason = reason,
            Lines =
            [
                new StockLineRequest
                {
                    IngredientId = ingredientId,
                    Quantity = quantity,
                    RowVersion = rowVersion
                }
            ]
        };
    }

    private static async Task<bool> TryWriteStockConcurrentlyAsync(
        DbContextOptions<BranchFlowDbContext> options,
        int actorId,
        StockWriteRequest request)
    {
        try
        {
            await WriteStockAsync(options, actorId, request);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            // Request bị từ chối nếu tồn kho đã thay đổi khiến số lượng không đủ.
            return false;
        }
    }

    private static async Task WriteStockAsync(
        DbContextOptions<BranchFlowDbContext> options,
        int actorId,
        StockWriteRequest request,
        bool employee = false)
    {
        await using var db = new BranchFlowDbContext(options);

        TimeProvider clock = TimeProvider.System;
        var authRepository = new AuthRepository(db, clock);
        var accessService = new BranchAccessService(authRepository);
        var repository = new MvpRepository(db);
        var unitOfWork = new UnitOfWork(db);

        var service = new MvpService(
            repository,
            unitOfWork,
            accessService,
            new PasswordHasher<AppUser>(),
            clock);

        await service.WriteStockAsync(
            actorId,
            employee,
            request,
            CancellationToken.None);
    }

    private async Task<(decimal Quantity, string RowVersion)> ReadBalanceAsync(
        DbContextOptions<BranchFlowDbContext> options,
        int ingredientId)
    {
        await using var db = new BranchFlowDbContext(options);

        var balance = await db.BranchIngredients
            .AsNoTracking()
            .SingleAsync(x => x.BranchId == BranchId && x.IngredientId == ingredientId);

        return (balance.Quantity, Convert.ToBase64String(balance.RowVersion));
    }

    private static async Task<int> CountStockTransactionsAsync(
        DbContextOptions<BranchFlowDbContext> options,
        int ingredientId)
    {
        await using var db = new BranchFlowDbContext(options);

        return await db.StockTransactionDetails
            .CountAsync(x => x.IngredientId == ingredientId);
    }

    private static async Task CleanupAsync(
        DbContextOptions<BranchFlowDbContext> options,
        int ingredientId)
    {
        await using var db = new BranchFlowDbContext(options);

        var transactionIds = await db.StockTransactionDetails
            .Where(x => x.IngredientId == ingredientId)
            .Select(x => x.StockTransactionId)
            .Distinct()
            .ToListAsync();

        if (transactionIds.Count > 0)
        {
            await db.AuditLogs
                .Where(x => x.EntityName == "StockTransaction" &&
                            transactionIds.Contains(x.EntityId))
                .ExecuteDeleteAsync();

            await db.StockTransactionDetails
                .Where(x => transactionIds.Contains(x.StockTransactionId))
                .ExecuteDeleteAsync();

            await db.StockTransactions
                .Where(x => transactionIds.Contains(x.Id))
                .ExecuteDeleteAsync();
        }

        await db.BranchIngredients
            .Where(x => x.IngredientId == ingredientId)
            .ExecuteDeleteAsync();

        await db.Ingredients
            .Where(x => x.Id == ingredientId)
            .ExecuteDeleteAsync();
    }
}
