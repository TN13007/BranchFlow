using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Smoke độc lập: chỉ DB test local, fixture có ID riêng và được dọn trong finally.
var root = Path.GetFullPath(args.Single());
var connection = Environment.GetEnvironmentVariable("BRANCHFLOW_MENU_TEST_DB") ??
    @"Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True";
var settings = new SqlConnectionStringBuilder(connection);
if (settings.InitialCatalog != "BranchFlow_Test" ||
    !(settings.DataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
      settings.DataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("Chỉ chạy trên localhost và BranchFlow_Test.");
await using var db = new BranchFlowDbContext(new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options);
var created = new List<object>();
T Add<T>(T item) where T : class { db.Add(item); created.Add(item); return item; }
var suffix = Guid.NewGuid().ToString("N")[..12];
var now = DateTime.UtcNow;
Process? host = null;
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
var logs = new StringBuilder();
var passed = 0;
try
{
    var employeeRole = await db.Roles.SingleAsync(r => r.Code == "EMPLOYEE" && !r.Deleted);
    var ownerRole = await db.Roles.SingleAsync(r => r.Code == "OWNER" && !r.Deleted);
    var size = await db.Sizes.FirstAsync(s => s.IsActive && !s.Deleted);
    var user = Add(new AppUser { RoleId = employeeRole.Id, Username = "smoke_" + suffix,
        PasswordHash = "TEST_ONLY_NOT_A_LOGIN_HASH", FullName = "Menu smoke", IsActive = true });
    var owner = Add(new AppUser { RoleId = ownerRole.Id, Username = "owner_" + suffix,
        PasswordHash = "TEST_ONLY_NOT_A_LOGIN_HASH", FullName = "Menu smoke owner", IsActive = true });
    var branch = Add(new Branch { Code = "SM" + suffix, Name = "Menu smoke " + suffix, Address = "Local test", IsActive = true });
    var other = Add(new Branch { Code = "OT" + suffix, Name = "Other smoke " + suffix, Address = "Local test", IsActive = true });
    var profile = Add(new EmployeeProfile { User = user, EmployeeCode = "SM" + suffix, HireDate = DateOnly.FromDateTime(now) });
    var assignment = Add(new EmployeeBranchAssignment { EmployeeProfile = profile, Branch = branch,
        AssignedByUser = owner, EffectiveFrom = now.AddDays(-1) });
    var session = Add(new AuthSession { Id = Guid.NewGuid(), User = user, CreatedAt = now.AddMinutes(-1), ExpiresAt = now.AddHours(1) });
    var ownerSession = Add(new AuthSession { Id = Guid.NewGuid(), User = owner, CreatedAt = now.AddMinutes(-1), ExpiresAt = now.AddHours(1) });
    var category = Add(new Category { Name = "Smoke " + suffix, IsActive = true });
    var product = Add(new Product { Category = category, Name = "Tea " + suffix, IsActive = true });
    var productSize = Add(new ProductSize { Product = product, SizeId = size.Id, Price = 30000.50m, IsActive = true });
    var available = Add(new BranchProduct { Branch = branch, Product = product, IsAvailable = true });
    var toppingGroup = Add(new ToppingGroup { Name = "Smoke " + suffix, IsActive = true });
    var topping = Add(new Topping { ToppingGroup = toppingGroup, Name = "Pearl " + suffix, Price = 5000m, IsActive = true });
    var toppingAvailable = Add(new BranchTopping { Branch = branch, Topping = topping, IsAvailable = true });
    var notes = Add(new NoteGroup { Name = "Smoke " + suffix, IsActive = true });
    var option = Add(new NoteOption { NoteGroup = notes, Name = "Less sugar", IsActive = true });
    await db.SaveChangesAsync();

    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = root, UseShellExecute = false,
        CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var arg in new[] { Path.Combine(root, "src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll"),
        "--urls", "http://localhost:19117", "--contentRoot", Path.Combine(root, "src/BranchFlow.Api") }) start.ArgumentList.Add(arg);
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    start.Environment["ConnectionStrings__BranchFlowDb"] = connection;
    start.Environment["Jwt__Key"] = key;
    start.Environment["Jwt__Issuer"] = "MenuSmoke";
    start.Environment["Jwt__Audience"] = "MenuSmoke";
    host = Process.Start(start) ?? throw new InvalidOperationException("Không mở được API test host.");
    host.OutputDataReceived += (_, e) => { lock (logs) logs.AppendLine(e.Data); };
    host.ErrorDataReceived += (_, e) => { lock (logs) logs.AppendLine(e.Data); };
    host.BeginOutputReadLine(); host.BeginErrorReadLine();
    using var http = new HttpClient { BaseAddress = new Uri("http://localhost:19117"), Timeout = TimeSpan.FromSeconds(10) };
    var ready = false;
    for (var i = 0; i < 80; i++)
    {
        if (host.HasExited) throw new InvalidOperationException("API test host đã dừng.");
        try { using var probe = await http.GetAsync("/api/order-menu"); ready = true; break; }
        catch (HttpRequestException) { await Task.Delay(100); }
    }
    if (!ready) throw new InvalidOperationException("API test host chưa sẵn sàng.");
    string Token(AppUser u, AuthSession s, string role, bool expired = false) => new JwtSecurityTokenHandler().WriteToken(
        new JwtSecurityToken("MenuSmoke", "MenuSmoke", [new Claim("sub", u.Id.ToString()), new Claim("sid", s.Id.ToString()), new Claim("role", role)],
            now.AddMinutes(-5), now.AddMinutes(expired ? -1 : 10),
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
    var token = Token(user, session, "EMPLOYEE");
    async Task<JsonElement> Check(int status, string? bearer = null, string? query = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/order-menu" + (query ?? $"?branchId={branch.Id}"));
        if (bearer != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        using var response = await http.SendAsync(request);
        if ((int)response.StatusCode != status) throw new Exception($"Expected HTTP {status}, actual {(int)response.StatusCode}.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (status >= 400 && (response.Content.Headers.ContentType?.MediaType != "application/problem+json" ||
            json.RootElement.GetProperty("status").GetInt32() != status || !json.RootElement.TryGetProperty("traceId", out _)))
            throw new Exception("Lỗi không đúng ProblemDetails/traceId.");
        passed++; Console.WriteLine($"PASS HTTP {status} case {passed}");
        return json.RootElement.Clone();
    }
    await Check(401); await Check(401, "invalid"); await Check(401, Token(user, session, "EMPLOYEE", true));
    await Check(403, Token(owner, ownerSession, "OWNER"));
    await Check(403, token, $"?branchId={other.Id}");
    await Check(400, token, ""); await Check(400, token, "?branchId=0"); await Check(400, token, "?branchId=abc");
    var menu = await Check(200, token);
    var sizes = menu.GetProperty("categories")[0].GetProperty("products")[0].GetProperty("productSizes");
    if (sizes[0].GetProperty("id").GetInt32() != productSize.Id || sizes[0].GetProperty("price").GetDecimal() != 30000.50m ||
        menu.GetProperty("toppingGroups")[0].GetProperty("toppings")[0].GetProperty("maxQuantity").GetInt32() != 3)
        throw new Exception("Sai ID/giá/topping trong menu SQL thật.");
    available.IsAvailable = false; toppingAvailable.IsAvailable = false; option.Deleted = true; await db.SaveChangesAsync();
    menu = await Check(200, token);
    if (menu.GetProperty("categories").GetArrayLength() != 0 || menu.GetProperty("toppingGroups").GetArrayLength() != 0 ||
        menu.GetProperty("noteGroups").EnumerateArray().Any(g => g.GetProperty("id").GetInt32() == notes.Id)) throw new Exception("Lộ dữ liệu tắt/xóa.");
    available.IsAvailable = true; product.Deleted = true; await db.SaveChangesAsync();
    menu = await Check(200, token);
    if (menu.GetProperty("categories").GetArrayLength() != 0) throw new Exception("Lộ sản phẩm bị xóa mềm.");
    branch.IsActive = false; await db.SaveChangesAsync(); await Check(404, token);
    branch.IsActive = true; assignment.EffectiveTo = now.AddMinutes(-1); await db.SaveChangesAsync(); await Check(403, token);
    assignment.EffectiveTo = null; assignment.EffectiveFrom = now.AddHours(1); await db.SaveChangesAsync(); await Check(403, token);
    assignment.EffectiveFrom = now.AddDays(-1); user.IsActive = false; await db.SaveChangesAsync(); await Check(401, token);
    user.IsActive = true; session.RevokedAt = now; await db.SaveChangesAsync(); await Check(401, token);
    Console.WriteLine($"Menu SQL/HTTP smoke: {passed} cases passed.");
}
finally
{
    if (host is not null) { if (!host.HasExited) { host.Kill(true); await host.WaitForExitAsync(); } host.Dispose(); }
    // Chỉ xóa đúng các entity do lần chạy này tạo, không reset DB hoặc dữ liệu khác.
    db.ChangeTracker.Clear();
    foreach (var entity in created.AsEnumerable().Reverse())
    {
        var entry = db.Entry(entity);
        if (!entry.IsKeySet) continue;
        entry.State = EntityState.Deleted;
        await db.SaveChangesAsync();
        entry.State = EntityState.Detached;
    }
    Console.WriteLine("Cleaned this run's SQL fixtures.");
}
