using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var root = Path.GetFullPath(args.Single());
var connection = Environment.GetEnvironmentVariable("BRANCHFLOW_REVENUE_TEST_DB") ??
    @"Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True";

var settings = new SqlConnectionStringBuilder(connection);
if (settings.InitialCatalog != "BranchFlow_Test" ||
    !(settings.DataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
      settings.DataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("Chi cho phep chay tren localhost va BranchFlow_Test.");

await using var db = new BranchFlowDbContext(
    new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options);

var created = new List<object>();
T Add<T>(T item) where T : class { db.Add(item); created.Add(item); return item; }

var suffix = Guid.NewGuid().ToString("N")[..12];
var now = DateTime.UtcNow;
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
var passed = 0;
Process? host = null;

var from = new DateOnly(2026, 11, 8);
var to = new DateOnly(2026, 11, 9);

try
{
    var ownerRole = await db.Roles.SingleAsync(r => r.Code == "OWNER" && !r.Deleted);
    var systemAdminRole = await db.Roles.SingleAsync(r => r.Code == "SYSTEM_ADMIN" && !r.Deleted);
    var branchAdminRole = await db.Roles.SingleAsync(r => r.Code == "BRANCH_ADMIN" && !r.Deleted);
    var employeeRole = await db.Roles.SingleAsync(r => r.Code == "EMPLOYEE" && !r.Deleted);

    var hasher = new PasswordHasher<AppUser>();

    AppUser CreateUser(string roleName, int roleId)
    {
        var user = Add(new AppUser
        {
            RoleId = roleId,
            Username = roleName.ToLowerInvariant() + "_" + suffix,
            FullName = "Revenue smoke " + roleName,
            IsActive = true
        });

        user.PasswordHash = hasher.HashPassword(
            user, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        return user;
    }

    var owner = CreateUser("OWNER", ownerRole.Id);
    var systemAdmin = CreateUser("SYSTEM_ADMIN", systemAdminRole.Id);
    var branchAdmin = CreateUser("BRANCH_ADMIN", branchAdminRole.Id);
    var employee = CreateUser("EMPLOYEE", employeeRole.Id);

    var branchA = Add(new Branch
    {
        Code = "RA" + suffix,
        Name = "Revenue smoke A " + suffix,
        Address = "Local test",
        IsActive = true
    });

    var branchB = Add(new Branch
    {
        Code = "RB" + suffix,
        Name = "Revenue smoke B " + suffix,
        Address = "Local test",
        IsActive = true
    });

    Add(new AdminBranchAssignment
    {
        AdminUser = branchAdmin,
        Branch = branchA,
        AssignedByUser = owner,
        EffectiveFrom = now.AddDays(-1)
    });

    AuthSession CreateSession(AppUser user)
    {
        return Add(new AuthSession
        {
            Id = Guid.NewGuid(),
            User = user,
            CreatedAt = now.AddMinutes(-1),
            ExpiresAt = now.AddHours(1)
        });
    }

    var ownerSession = CreateSession(owner);
    var systemAdminSession = CreateSession(systemAdmin);
    var branchAdminSession = CreateSession(branchAdmin);
    var employeeSession = CreateSession(employee);

    // Vietnam local time:
    // 2026-11-08 23:59:59 = 2026-11-08 16:59:59 UTC.
    // 2026-11-09 00:00:00 = 2026-11-08 17:00:00 UTC.
    SalesOrder CreateOrder(
        Branch branch, AppUser creator, DateTime createdAt,
        DateOnly businessDate, int sequence, decimal amount, string status)
    {
        return Add(new SalesOrder
        {
            Branch = branch,
            CreatedByUser = creator,
            Code = "REV-" + Guid.NewGuid().ToString("N"),
            BusinessDate = businessDate,
            DailySequence = sequence,
            TotalAmount = amount,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });
    }

    CreateOrder(
        branchA, owner, new DateTime(2026, 11, 8, 16, 59, 59, DateTimeKind.Utc),
        new DateOnly(2026, 11, 8), 9001, 100.25m, "COMPLETED");

    CreateOrder(
        branchA, owner, new DateTime(2026, 11, 8, 17, 0, 0, DateTimeKind.Utc),
        new DateOnly(2026, 11, 9), 9002, 200.50m, "COMPLETED");

    CreateOrder(
        branchA, owner, new DateTime(2026, 11, 8, 17, 30, 0, DateTimeKind.Utc),
        new DateOnly(2026, 11, 9), 9003, 999.99m, "CANCELLED");

    CreateOrder(
        branchB, owner, new DateTime(2026, 11, 8, 18, 0, 0, DateTimeKind.Utc),
        new DateOnly(2026, 11, 9), 9001, 300.75m, "COMPLETED");

    await db.SaveChangesAsync();
    Console.WriteLine("Fixture creation completed.");

    var start = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = root,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    foreach (var arg in new[]
    {
        Path.Combine(root, "src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll"),
        "--urls", "http://localhost:19121",
        "--contentRoot", Path.Combine(root, "src/BranchFlow.Api")
    })
        start.ArgumentList.Add(arg);

    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    start.Environment["ConnectionStrings__BranchFlowDb"] = connection;
    start.Environment["Jwt__Key"] = key;
    start.Environment["Jwt__Issuer"] = "RevenueSmoke";
    start.Environment["Jwt__Audience"] = "RevenueSmoke";

    host = Process.Start(start) ??
        throw new InvalidOperationException("Khong mo duoc API test host.");

    var logs = new StringBuilder();
    host.OutputDataReceived += (_, e) => { lock (logs) logs.AppendLine(e.Data); };
    host.ErrorDataReceived += (_, e) => { lock (logs) logs.AppendLine(e.Data); };
    host.BeginOutputReadLine();
    host.BeginErrorReadLine();

    using var http = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:19121"),
        Timeout = TimeSpan.FromSeconds(10)
    };

    var ready = false;

    for (var i = 0; i < 80; i++)
    {
        if (host.HasExited)
            throw new InvalidOperationException("API test host da dung.\n" + logs);

        try
        {
            using var probe = await http.GetAsync(
                $"/api/management/revenue?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

            ready = true;
            break;
        }
        catch (HttpRequestException)
        {
            await Task.Delay(100);
        }
    }

    if (!ready)
        throw new InvalidOperationException("API test host chua san sang.");

    string Token(AppUser user, AuthSession session, string role, bool expired = false) =>
        new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                "RevenueSmoke",
                "RevenueSmoke",
                [
                    new Claim("sub", user.Id.ToString()),
                    new Claim("sid", session.Id.ToString()),
                    new Claim("role", role)
                ],
                now.AddMinutes(-5),
                now.AddMinutes(expired ? -1 : 10),
                new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256)));

    var ownerToken = Token(owner, ownerSession, "OWNER");
    var systemAdminToken = Token(systemAdmin, systemAdminSession, "SYSTEM_ADMIN");
    var branchAdminToken = Token(branchAdmin, branchAdminSession, "BRANCH_ADMIN");
    var employeeToken = Token(employee, employeeSession, "EMPLOYEE");

    string Query(int? branchId = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = $"?from={(startDate ?? from):yyyy-MM-dd}&to={(endDate ?? to):yyyy-MM-dd}";
        return branchId.HasValue ? query + $"&branchId={branchId.Value}" : query;
    }

    async Task<JsonElement> Check(int expectedStatus, string? bearer = null, string? query = null)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/management/revenue" + (query ?? Query()));

        if (bearer != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

        using var response = await http.SendAsync(request);
        var actualStatus = (int)response.StatusCode;

        if (actualStatus != expectedStatus)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"Expected HTTP {expectedStatus}, actual {actualStatus}. Body: {body}");
        }

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        if (expectedStatus >= 400 &&
            (response.Content.Headers.ContentType?.MediaType != "application/problem+json" ||
             json.RootElement.GetProperty("status").GetInt32() != expectedStatus ||
             !json.RootElement.TryGetProperty("traceId", out _)))
            throw new Exception("Error response khong dung ProblemDetails/traceId.");

        passed++;
        Console.WriteLine($"PASS HTTP {expectedStatus} case {passed}");

        return json.RootElement.Clone();
    }

    JsonElement[] Rows(JsonElement response) =>
        response.EnumerateArray().ToArray();

    int Orders(JsonElement[] rows) =>
        rows.Sum(x => x.GetProperty("orders").GetInt32());

    decimal Amount(JsonElement[] rows) =>
        rows.Sum(x => x.GetProperty("amount").GetDecimal());

    // Authentication and authorization.
    await Check(401);
    await Check(401, "invalid");
    await Check(401, Token(employee, employeeSession, "EMPLOYEE", true));
    await Check(403, employeeToken);

    // Global roles.
    var ownerRows = Rows(await Check(200, ownerToken));
    var systemRows = Rows(await Check(200, systemAdminToken));

    // Branch Admin scope.
    var branchAdminRows = Rows(await Check(200, branchAdminToken));

    if (branchAdminRows.Any(x => x.GetProperty("branchId").GetInt32() != branchA.Id))
        throw new Exception("Branch Admin nhin thay doanh thu ngoai chi nhanh duoc gan.");

    var branchAResult = Rows(await Check(200, branchAdminToken, Query(branchA.Id)));

    if (branchAResult.Any(x => x.GetProperty("branchId").GetInt32() != branchA.Id))
        throw new Exception("Branch Admin query chi nhanh duoc gan tra ve sai du lieu.");

    await Check(403, branchAdminToken, Query(branchB.Id));

    // Invalid date range.
    await Check(400, ownerToken, Query(null,
        new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 8)));

    // Expected global revenue.
    if (Orders(ownerRows) != 3 || Amount(ownerRows) != 601.50m)
        throw new Exception(
            $"System revenue sai: orders={Orders(ownerRows)}, amount={Amount(ownerRows)}.");

    var day8 = ownerRows.Single(x =>
        x.GetProperty("branchId").GetInt32() == branchA.Id &&
        x.GetProperty("date").GetString() == "2026-11-08");

    var day9BranchA = ownerRows.Single(x =>
        x.GetProperty("branchId").GetInt32() == branchA.Id &&
        x.GetProperty("date").GetString() == "2026-11-09");

    if (day8.GetProperty("orders").GetInt32() != 1 ||
        day8.GetProperty("amount").GetDecimal() != 100.25m)
        throw new Exception("Don truoc 00:00 VN khong duoc gom vao ngay 2026-11-08.");

    if (day9BranchA.GetProperty("orders").GetInt32() != 1 ||
        day9BranchA.GetProperty("amount").GetDecimal() != 200.50m)
        throw new Exception("Don tai 00:00 VN khong duoc gom vao ngay 2026-11-09.");

    if (ownerRows.Any(x => x.GetProperty("amount").GetDecimal() == 999.99m))
        throw new Exception("Don CANCELLED van bi tinh vao doanh thu.");

    if (Orders(systemRows) != Orders(ownerRows) ||
        Amount(systemRows) != Amount(ownerRows))
        throw new Exception("Owner va System Admin co ket qua doanh thu khac nhau.");

    // System total equals sum of branch-specific results.
    var branchBResult = Rows(await Check(200, ownerToken, Query(branchB.Id)));

    var summedOrders = Orders(branchAResult) + Orders(branchBResult);
    var summedAmount = Amount(branchAResult) + Amount(branchBResult);

    if (summedOrders != Orders(ownerRows) || summedAmount != Amount(ownerRows))
        throw new Exception("Tong doanh thu cac chi nhanh khong bang tong he thong.");

    Console.WriteLine("PASS: OWNER and SYSTEM_ADMIN can read system revenue.");
    Console.WriteLine("PASS: BRANCH_ADMIN is restricted to assigned branch.");
    Console.WriteLine("PASS: EMPLOYEE cannot access management revenue.");
    Console.WriteLine("PASS: COMPLETED included, CANCELLED excluded.");
    Console.WriteLine("PASS: Vietnam midnight boundary and date grouping.");
    Console.WriteLine("PASS: System revenue equals sum of branch revenue.");
    Console.WriteLine($"Revenue SQL/HTTP smoke: {passed} cases passed.");
    Console.WriteLine($"System total: {Orders(ownerRows)} orders, {Amount(ownerRows):N2}.");
}
catch (Exception ex)
{
    Console.WriteLine("REVENUE SMOKE ORIGINAL ERROR:");
    Console.WriteLine(ex);
    throw;
}
finally
{
    if (host is not null)
    {
        if (!host.HasExited)
        {
            host.Kill(true);
            await host.WaitForExitAsync();
        }

        host.Dispose();
    }

    // Chi xoa entity do chinh lan chay nay tao.
    db.ChangeTracker.Clear();

    foreach (var entity in created.AsEnumerable().Reverse())
    {
        var entry = db.Entry(entity);
        if (!entry.IsKeySet) continue;

        try
        {
            if (entity is AuthSession session)
            {
                await db.RefreshTokens
                    .Where(x => x.SessionId == session.Id)
                    .ExecuteDeleteAsync();

                var deleted = await db.AuthSessions
                    .Where(x => x.Id == session.Id)
                    .ExecuteDeleteAsync();

                Console.WriteLine(
                    $"CLEANUP AuthSession: {session.Id}, deleted={deleted}");

                continue;
            }

            entry.State = EntityState.Deleted;
            await db.SaveChangesAsync();
            entry.State = EntityState.Detached;

            Console.WriteLine($"CLEANUP PASS: {entity.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CLEANUP ERROR: {entity.GetType().Name}");
            Console.WriteLine(ex);
            throw;
        }
    }

    Console.WriteLine("Cleaned this run's SQL fixtures.");
}