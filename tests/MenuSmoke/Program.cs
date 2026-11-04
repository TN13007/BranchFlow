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
using Microsoft.AspNetCore.Identity;
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
    var size = Add(new Size { Name = "S" + suffix, IsActive = true });
    var hasher = new PasswordHasher<AppUser>();
    var user = Add(new AppUser { RoleId = employeeRole.Id, Username = "smoke_" + suffix,
        FullName = "Menu smoke", IsActive = true });
    var owner = Add(new AppUser { RoleId = ownerRole.Id, Username = "owner_" + suffix,
        FullName = "Menu smoke owner", IsActive = true });
    user.PasswordHash = hasher.HashPassword(user, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    owner.PasswordHash = hasher.HashPassword(owner, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    var branch = Add(new Branch { Code = "SM" + suffix, Name = "Menu smoke " + suffix, Address = "Local test", IsActive = true });
    var other = Add(new Branch { Code = "OT" + suffix, Name = "Other smoke " + suffix, Address = "Local test", IsActive = true });
    var profile = Add(new EmployeeProfile { User = user, EmployeeCode = "SM" + suffix, HireDate = DateOnly.FromDateTime(now) });
    var assignment = Add(new EmployeeBranchAssignment { EmployeeProfile = profile, Branch = branch,
        AssignedByUser = owner, EffectiveFrom = now.AddDays(-1) });
    var session = Add(new AuthSession { Id = Guid.NewGuid(), User = user, CreatedAt = now.AddMinutes(-1), ExpiresAt = now.AddHours(1) });
    var ownerSession = Add(new AuthSession { Id = Guid.NewGuid(), User = owner, CreatedAt = now.AddMinutes(-1), ExpiresAt = now.AddHours(1) });
    var category = Add(new Category { Name = "Smoke " + suffix, IsActive = true });
    var product = Add(new Product { Category = category, Name = "Tea " + suffix, IsActive = true });
    var productSize = Add(new ProductSize { Product = product, Size = size, Price = 30000.50m, IsActive = true });
    var available = Add(new BranchProduct { Branch = branch, Product = product, IsAvailable = true });
    var toppingGroup = Add(new ToppingGroup { Name = "Smoke " + suffix, IsActive = true });
    var topping = Add(new Topping { ToppingGroup = toppingGroup, Name = "Pearl " + suffix, Price = 5000m, IsActive = true });
    var toppingAvailable = Add(new BranchTopping { Branch = branch, Topping = topping, IsAvailable = true });
    var notes = Add(new NoteGroup { Name = "Smoke " + suffix, IsActive = true });
    var option = Add(new NoteOption { NoteGroup = notes, Name = "Less sugar", IsActive = true });
    // Nhóm ghi chú độc lập chứng minh test không giả định DB chỉ chứa fixture chính.
    var controlNotes = Add(new NoteGroup { Name = "Other notes " + suffix, IsActive = true });
    var controlOption = Add(new NoteOption { NoteGroup = controlNotes, Name = "Keep visible", IsActive = true });
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
        if (status == 200 && !json.RootElement.GetProperty("noteGroups").EnumerateArray().Any(g =>
            g.GetProperty("id").GetInt32() == controlNotes.Id &&
            g.GetProperty("options").EnumerateArray().Any(o => o.GetProperty("id").GetInt32() == controlOption.Id)))
            throw new Exception("Nhóm ghi chú độc lập bị mất khỏi menu.");
        passed++; Console.WriteLine($"PASS HTTP {status} case {passed}");
        return json.RootElement.Clone();
    }
    await Check(401); await Check(401, "invalid"); await Check(401, Token(user, session, "EMPLOYEE", true));
    await Check(403, Token(owner, ownerSession, "OWNER"));
    await Check(403, token, $"?branchId={other.Id}");
    await Check(403, token, "?branchId=2147483647");
    await Check(400, token, ""); await Check(400, token, "?branchId=0");
    await Check(400, token, "?branchId=-1"); await Check(400, token, "?branchId=abc");
    var menu = await Check(200, token);
    if (menu.GetProperty("branch").GetProperty("id").GetInt32() != branch.Id ||
        menu.GetProperty("categories").GetArrayLength() != 1 ||
        menu.GetProperty("toppingGroups").GetArrayLength() != 1 ||
        !menu.GetProperty("noteGroups").EnumerateArray().Any(g => g.GetProperty("id").GetInt32() == notes.Id))
        throw new Exception("Menu không khớp branch/collection của fixture.");
    var sizes = menu.GetProperty("categories")[0].GetProperty("products")[0].GetProperty("productSizes");
    if (sizes[0].GetProperty("id").GetInt32() != productSize.Id || sizes[0].GetProperty("price").GetDecimal() != 30000.50m ||
        menu.GetProperty("categories")[0].GetProperty("products")[0].GetProperty("minPrice").GetDecimal() != 30000.50m ||
        menu.GetProperty("toppingGroups")[0].GetProperty("toppings")[0].GetProperty("maxQuantity").GetInt32() != 3 ||
        menu.GetProperty("noteGroups").EnumerateArray().Single(g => g.GetProperty("id").GetInt32() == notes.Id)
            .GetProperty("maxSelections").GetInt32() != 1)
        throw new Exception("Sai ID/giá/topping trong menu SQL thật.");
    async Task AssertEmptyCategoryAsync()
    {
        var result = await Check(200, token);
        if (result.GetProperty("categories").GetArrayLength() != 0)
            throw new Exception("Món/size/category không hợp lệ vẫn hiện trong menu.");
    }
    async Task AssertEmptyToppingGroupAsync()
    {
        var result = await Check(200, token);
        if (result.GetProperty("toppingGroups").GetArrayLength() != 0)
            throw new Exception("Topping không hợp lệ vẫn hiện trong menu.");
    }
    async Task AssertEmptyNoteGroupAsync()
    {
        var result = await Check(200, token);
        if (result.GetProperty("noteGroups").EnumerateArray().Any(g => g.GetProperty("id").GetInt32() == notes.Id))
            throw new Exception("Note không hợp lệ vẫn hiện trong menu.");
    }
    category.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    category.IsActive = true; category.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    category.Deleted = false; product.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    product.IsActive = true; productSize.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    productSize.IsActive = true; productSize.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    productSize.Deleted = false; size.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    size.IsActive = true; size.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    size.Deleted = false; await db.SaveChangesAsync();
    toppingGroup.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyToppingGroupAsync();
    toppingGroup.IsActive = true; topping.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyToppingGroupAsync();
    topping.IsActive = true; topping.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyToppingGroupAsync();
    topping.Deleted = false; await db.SaveChangesAsync();
    notes.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyNoteGroupAsync();
    notes.IsActive = true; notes.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyNoteGroupAsync();
    notes.Deleted = false; option.IsActive = false; await db.SaveChangesAsync(); await AssertEmptyNoteGroupAsync();
    option.IsActive = true; await db.SaveChangesAsync();
    available.IsAvailable = false; await db.SaveChangesAsync(); await AssertEmptyCategoryAsync();
    available.IsAvailable = true; toppingAvailable.IsAvailable = false; await db.SaveChangesAsync(); await AssertEmptyToppingGroupAsync();
    toppingAvailable.IsAvailable = true; option.Deleted = true; await db.SaveChangesAsync(); await AssertEmptyNoteGroupAsync();
    option.Deleted = false; product.Deleted = true; await db.SaveChangesAsync();
    menu = await Check(200, token);
    if (menu.GetProperty("categories").GetArrayLength() != 0) throw new Exception("Lộ sản phẩm bị xóa mềm.");
    product.Deleted = false; branch.IsActive = false; await db.SaveChangesAsync(); await Check(404, token);
    branch.IsActive = true; branch.Deleted = true; await db.SaveChangesAsync(); await Check(404, token);
    branch.Deleted = false; await db.SaveChangesAsync();
    branch.IsActive = true; assignment.EffectiveTo = now.AddMinutes(-1); await db.SaveChangesAsync(); await Check(403, token);
    assignment.EffectiveTo = null; assignment.EffectiveFrom = now.AddHours(1); await db.SaveChangesAsync(); await Check(403, token);
    assignment.EffectiveFrom = now.AddDays(-1); user.IsActive = false; await db.SaveChangesAsync(); await Check(401, token);
    user.IsActive = true; session.RevokedAt = now; await db.SaveChangesAsync(); await Check(401, token);
    Console.WriteLine("PASS QA-S4-019: menu branch, ProductSizeId, current price and option structure");
    Console.WriteLine("PASS QA-S4-020: active/deleted and branch availability filters");
    Console.WriteLine("PASS QA-S4-021: missing, zero, negative and malformed branchId");
    Console.WriteLine("PASS QA-S4-022: nonexistent/out-of-scope branch 403, assigned inactive branch 404");
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
