using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BranchFlow.Application.DTOs.Mvp;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

// Isolated localhost API, genuine JWT/session/role middleware and real local SQL.
// Synthetic test sessions do not test password login. Never use a production key.
string connection = Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set BRANCHFLOW_TEST_CONNECTION_STRING.");
var sql = new SqlConnectionStringBuilder(connection);
if (!string.Equals(sql.InitialCatalog, "BranchFlow_Test", StringComparison.OrdinalIgnoreCase) ||
    !System.Text.RegularExpressions.Regex.IsMatch(sql.DataSource, @"^(localhost|127\.0\.0\.1|\.)(\\[A-Za-z0-9_]+)?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) ||
    sql.MultipleActiveResultSets)
    throw new InvalidOperationException("Only a local BranchFlow_Test without MARS is allowed.");
if ((args.Length != 1 && args.Length != 2) || !File.Exists(args[0]) ||
    (args.Length == 2 && args[1] != "--cross-day"))
    throw new ArgumentException("Pass the built BranchFlow.Api.dll path, optionally followed by --cross-day.");
bool crossDay = args.Length == 2;
var options = new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options;
BranchFlowDbContext Db() => new(options);
int checks = 0;
void Check(bool passed, string label) { if (!passed) throw new Exception(label); Console.WriteLine($"PASS {++checks:D2} {label}"); }
var sessionIds = new List<Guid>();
Process? host = null;
int orderId = 0;
int productId = 0, categoryId = 0, toppingGroupId = 0, noteGroupId = 0;
string code = "WBS3421-" + Guid.NewGuid().ToString("N")[..12];
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
const string issuer = "BranchFlow.WBS3421.LocalTest";
const string audience = "BranchFlow.WBS3421.LocalTest";
var tokens = new Dictionary<string, string>();
var actors = new Dictionary<string, int>();
var apiOutput = new StringBuilder();
try
{
    await using (var db = Db())
    {
        var branch = await db.Branches.SingleAsync(x => x.Code == "BF-DEMO-01" && x.IsActive && !x.Deleted);
        foreach (var name in new[] { "demo.systemadmin", "demo.branchadmin.01", "demo.branchadmin.02", "demo.employee.a", "demo.owner" })
        {
            var user = await db.AppUsers.Include(x => x.Role).SingleAsync(x => x.Username == name && x.IsActive && !x.Deleted);
            var sid = Guid.NewGuid(); sessionIds.Add(sid); actors[name] = user.Id;
            db.AuthSessions.Add(new AuthSession { Id = sid, UserId = user.Id, CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(30) });
            var jwt = new JwtSecurityToken(issuer, audience, [new Claim("sub", user.Id.ToString()), new Claim("sid", sid.ToString()), new Claim("role", user.Role.Code)],
                DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddMinutes(25), new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
            tokens[name] = new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        var createdAt = DateTime.UtcNow.AddDays(crossDay ? -1 : 0);
        var day = DateOnly.FromDateTime(createdAt.AddHours(7));
        int sequence = (await db.SalesOrders.Where(x => x.BranchId == branch.Id && x.BusinessDate == day).MaxAsync(x => (int?)x.DailySequence) ?? 0) + 1000000;
        var category = new Category { Name = code, IsActive = true };
        var product = new Product { Name = code, Category = category, IsActive = true };
        var size = await db.Sizes.SingleAsync(x => x.Name == "M" && !x.Deleted);
        var price = new ProductSize { Product = product, SizeId = size.Id, Price = 30000, IsActive = true };
        var group = new ToppingGroup { Name = code, IsActive = true };
        var pearl = new Topping { Name = code + " pearl", Price = 5000, ToppingGroup = group, IsActive = true };
        var jelly = new Topping { Name = code + " jelly", Price = 4000, ToppingGroup = group, IsActive = true };
        var notes = new NoteGroup { Name = code, IsActive = true };
        var note = new NoteOption { Name = code, NoteGroup = notes, IsActive = true };
        var line = new OrderItem { ProductSize = price, ProductNameSnapshot = product.Name,
            SizeNameSnapshot = "M", Quantity = 2, UnitPriceSnapshot = 30000, SubtotalAmount = 98000,
            Note = "WBS3421 rollback note",
            OrderItemToppings = [
                new OrderItemTopping { Topping = pearl, ToppingNameSnapshot = pearl.Name, Quantity = 3, UnitPriceSnapshot = 5000 },
                new OrderItemTopping { Topping = jelly, ToppingNameSnapshot = jelly.Name, Quantity = 1, UnitPriceSnapshot = 4000 }],
            OrderItemNotes = [new OrderItemNote { NoteOption = note, NoteNameSnapshot = note.Name }] };
        var order = new SalesOrder { Code = code, BranchId = branch.Id, CreatedByUserId = actors["demo.employee.a"],
            BusinessDate = day, DailySequence = sequence, CreatedAt = createdAt, Status = "COMPLETED", TotalAmount = 98000, OrderItems = [line] };
        db.SalesOrders.Add(order); await db.SaveChangesAsync(); orderId = order.Id;
        productId = product.Id; categoryId = category.Id; toppingGroupId = group.Id; noteGroupId = notes.Id;
        Console.WriteLine($"FIXTURE {code} ID={orderId}; isolated menu/order fixture; direct test fixture, not order-code allocator output.");
    }
    var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    start.ArgumentList.Add(Path.GetFullPath(args[0]));
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
    start.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:17207";
    start.Environment["ConnectionStrings__BranchFlowDb"] = connection;
    start.Environment["Jwt__Key"] = key; start.Environment["Jwt__Issuer"] = issuer; start.Environment["Jwt__Audience"] = audience;
    start.Environment["Logging__LogLevel__Default"] = "Critical";
    host = Process.Start(start) ?? throw new Exception("Cannot start isolated API.");
    host.OutputDataReceived += (_, e) => { lock (apiOutput) apiOutput.AppendLine(e.Data); };
    host.ErrorDataReceived += (_, e) => { lock (apiOutput) apiOutput.AppendLine(e.Data); };
    host.BeginOutputReadLine(); host.BeginErrorReadLine();
    using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:17207/"), Timeout = TimeSpan.FromSeconds(15) };
    bool ready = false;
    for (int i = 0; i < 60; i++)
    {
        if (host.HasExited) throw new Exception("Isolated API exited before becoming ready.");
        try { using var ping = await client.GetAsync("api/management/orders/" + orderId); ready = (int)ping.StatusCode == 401; if (ready) break; }
        catch (HttpRequestException) { }
        await Task.Delay(250);
    }
    Check(ready, "isolated API starts with normal authentication, anonymous request returns401");
    async Task<JsonElement> Request(string actor, string method, string path, object? body, int expected)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (actor != "anonymous") request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens[actor]);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        string text = await response.Content.ReadAsStringAsync();
        Check((int)response.StatusCode == expected, $"HTTP {actor} {method} {path} expected={expected} actual={(int)response.StatusCode}");
        return string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
    }
    string path = $"api/management/orders/{orderId}";
    async Task<JsonElement> Read() => await Request("demo.branchadmin.01", "GET", path, null, 200);
    object Change(JsonElement view, int qty, string reason = "WBS3421 quantity", bool cancel = false) => new {
        reason, cancel, rowVersion = view.GetProperty("rowVersion").GetString(),
        items = cancel ? Array.Empty<object>() : new object[] { new { itemId = view.GetProperty("items")[0].GetProperty("id").GetInt32(), quantity = qty } } };
    async Task<string> Snapshot()
    {
        await using var db = Db(); var o = await new MvpRepository(db).OrderAsync(orderId, default) ?? throw new Exception("Missing order");
        return JsonSerializer.Serialize(new { o.Status, o.TotalAmount, o.UpdatedAt, RowVersion = Convert.ToBase64String(o.RowVersion),
            Items = o.OrderItems.OrderBy(x => x.Id).Select(x => new { x.Id, x.Quantity, x.ProductSizeId, x.UnitPriceSnapshot, x.SubtotalAmount, x.Note,
                Toppings = x.OrderItemToppings.OrderBy(t => t.Id).Select(t => new { t.Id, t.ToppingId, t.Quantity, t.UnitPriceSnapshot, t.ToppingNameSnapshot }),
                Notes = x.OrderItemNotes.OrderBy(n => n.Id).Select(n => new { n.Id, n.NoteOptionId, n.NoteNameSnapshot }) }),
            History = o.OrderAdjustments.OrderBy(x => x.Id).Select(x => new { x.Id, x.Reason, x.BeforeData, x.AfterData, x.AdjustedByUserId, x.CreatedAt }) });
    }
    async Task Rejected(string actor, object input, int status)
    {
        var before = await Snapshot(); await Request(actor, "PUT", path, input, status);
        Check(before == await Snapshot(), $"SQL unchanged header/detail/topping/note/history/version after rejected {actor} {status}");
    }
    var initial = await Read();
    await Request("demo.systemadmin", "GET", path, null, 200);
    await Request("demo.branchadmin.02", "GET", path, null, 403);
    await Rejected("anonymous", Change(initial, 3), 401);
    foreach (var denied in new[] { "demo.employee.a", "demo.owner", "demo.branchadmin.02" }) await Rejected(denied, Change(initial, 3), 403);
    await Rejected("demo.branchadmin.01", Change(initial, 3, ""), 400);
    await Rejected("demo.branchadmin.01", Change(initial, 0), 400);
    await Rejected("demo.branchadmin.01", Change(initial, 1000), 400);
    await Request("demo.branchadmin.01", "PUT", path, Change(initial, 3), 200);
    var adjusted = await Read();
    Check(adjusted.GetProperty("totalAmount").GetDecimal() == 147000 && adjusted.GetProperty("history").GetArrayLength() == 1, "SQL-backed API total147000 and one history after branch admin2->3");
    await Rejected("demo.branchadmin.01", Change(initial, 4), 409);
    await Request("demo.systemadmin", "PUT", path, Change(adjusted, 4, "WBS3421 system quantity"), 200);
    var current = await Read();
    Check(current.GetProperty("totalAmount").GetDecimal() == 196000 && current.GetProperty("history").GetArrayLength() == 2, "system admin permitted; total196000 and two history");
    await Rejected("demo.branchadmin.01", Change(adjusted, 0, "WBS3421 stale cancel", true), 409);

    // Force genuine SQL FK547 inside normal EF SaveChanges transaction, not a mock rollback.
    foreach (bool historyFault in new[] { false, true })
    {
        string before = await Snapshot();
        var fault = new ForeignKeyFailure(historyFault);
        var faultOptions = new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).AddInterceptors(fault).Options;
        await using var db = new BranchFlowDbContext(faultOptions);
        var service = new MvpService(new MvpRepository(db), new UnitOfWork(db), new BranchAccessService(new AuthRepository(db, TimeProvider.System)), new PasswordHasher<AppUser>(), TimeProvider.System);
        bool sqlFailure = false;
        try { await service.AdjustOrderAsync(actors["demo.branchadmin.01"], orderId, new AdjustOrderRequest {
            Reason = "WBS3421 forced rollback", RowVersion = current.GetProperty("rowVersion").GetString()!,
            Items = [new QuantityChange { ItemId = current.GetProperty("items")[0].GetProperty("id").GetInt32(), Quantity = 5 }] }, default); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 }) { sqlFailure = true; }
        Check(sqlFailure && fault.Triggered, $"actual SQL FK547 injected in {(historyFault ? "history" : "detail")} save");
        Check(before == await Snapshot(), $"rollback restores header/detail/topping/note/history/version after {(historyFault ? "history" : "detail")} SQL failure");
    }
    async Task<decimal> Revenue(DateOnly? queryDay = null)
    {
        await using var db = Db(); var order = await db.SalesOrders.AsNoTracking().SingleAsync(x => x.Id == orderId);
        var day = (queryDay ?? DateOnly.FromDateTime(order.CreatedAt.AddHours(7))).ToString("yyyy-MM-dd");
        var rows = await Request("demo.systemadmin", "GET", $"api/management/revenue?branchId={order.BranchId}&from={day}&to={day}", null, 200);
        return rows.EnumerateArray().Sum(x => x.GetProperty("amount").GetDecimal());
    }
    decimal beforeCancel = await Revenue();
    DateOnly todayVietnam = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
    decimal todayBeforeCancel = crossDay ? await Revenue(todayVietnam) : 0;
    await Request("demo.branchadmin.01", "PUT", path, Change(current, 0, "WBS3421 cancel", true), 200);
    current = await Read();
    Check(current.GetProperty("status").GetString() == "CANCELLED" && current.GetProperty("history").GetArrayLength() == 3, "cancel retains order and adds third history");
    Check(beforeCancel - await Revenue() == 196000, "cancelled order196000 removed from actual HTTP revenue");
    if (crossDay)
    {
        await using var db = Db();
        var order = await db.SalesOrders.AsNoTracking().SingleAsync(x => x.Id == orderId);
        Check(DateOnly.FromDateTime(order.CreatedAt.AddHours(7)) < todayVietnam &&
            order.UpdatedAt.HasValue && DateOnly.FromDateTime(order.UpdatedAt.Value.AddHours(7)) == todayVietnam,
            "prior-day order was cancelled today through real API and persisted in SQL");
        Check(await Revenue(todayVietnam) == todayBeforeCancel,
            "cancel of prior-day order does not alter today's HTTP revenue");
    }
    await Rejected("demo.systemadmin", Change(current, 5), 409);
    await Rejected("demo.branchadmin.01", Change(current, 0, "WBS3421 cancel twice", true), 409);
    var own = await Request("demo.employee.a", "GET", $"api/orders/{orderId}", null, 200);
    Check(own.GetProperty("status").GetString() == "CANCELLED", "employee sees actual cancellation");
    await using (var db = Db())
    {
        var o = await new MvpRepository(db).OrderAsync(orderId, default) ?? throw new Exception("Missing fixture");
        var line = o.OrderItems.Single();
        Check(line.SizeNameSnapshot == "M" && line.UnitPriceSnapshot == 30000 && line.OrderItemToppings.Sum(t => t.Quantity * t.UnitPriceSnapshot) == 19000
            && line.Note == "WBS3421 rollback note" && line.OrderItemNotes.Count > 0, "SQL final size/topping prices/text and structured notes preserved");
    }
    Console.WriteLine($"PASS COMPLETE checks={checks}; fixture={code} ID={orderId} CANCELLED196000; no Dev writes, no application/schema changes.");
}
finally
{
    if (host is not null) { if (!host.HasExited) { host.Kill(entireProcessTree: true); await host.WaitForExitAsync(); } host.Dispose(); }
    await using var db = Db();
    await using var cleanup = await db.Database.BeginTransactionAsync();
    await db.OrderAdjustments.Where(x => x.SalesOrderId == orderId).ExecuteDeleteAsync();
    await db.OrderItemToppings.Where(x => x.OrderItem.SalesOrderId == orderId).ExecuteDeleteAsync();
    await db.OrderItemNotes.Where(x => x.OrderItem.SalesOrderId == orderId).ExecuteDeleteAsync();
    await db.OrderItems.Where(x => x.SalesOrderId == orderId).ExecuteDeleteAsync();
    await db.SalesOrders.Where(x => x.Id == orderId && x.Code == code).ExecuteDeleteAsync();
    await db.ProductSizes.Where(x => x.ProductId == productId).ExecuteDeleteAsync();
    await db.Products.Where(x => x.Id == productId && x.Name == code).ExecuteDeleteAsync();
    await db.Categories.Where(x => x.Id == categoryId && x.Name == code).ExecuteDeleteAsync();
    await db.Toppings.Where(x => x.ToppingGroupId == toppingGroupId).ExecuteDeleteAsync();
    await db.ToppingGroups.Where(x => x.Id == toppingGroupId && x.Name == code).ExecuteDeleteAsync();
    await db.NoteOptions.Where(x => x.NoteGroupId == noteGroupId).ExecuteDeleteAsync();
    await db.NoteGroups.Where(x => x.Id == noteGroupId && x.Name == code).ExecuteDeleteAsync();
    await db.AuthSessions.Where(x => sessionIds.Contains(x.Id)).ExecuteDeleteAsync();
    await cleanup.CommitAsync();
    Console.WriteLine("CLEANUP isolated API stopped; owned SQL fixtures/sessions removed; existing evidence orders and demo passwords unchanged.");
}

sealed class ForeignKeyFailure(bool history) : SaveChangesInterceptor
{
    public bool Triggered { get; private set; }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var db = data.Context!;
        if (history) db.ChangeTracker.Entries<OrderAdjustment>().Single(x => x.State == EntityState.Added).Entity.AdjustedByUserId = int.MaxValue;
        else db.ChangeTracker.Entries<OrderItem>().Single(x => x.State == EntityState.Modified).Entity.ProductSizeId = int.MaxValue;
        Triggered = true;
        return ValueTask.FromResult(result);
    }
}
