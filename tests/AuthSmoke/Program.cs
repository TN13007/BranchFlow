using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

// Runs against a built API from the supplied checkout. Only unique fixtures in local BranchFlow_Test are changed.
var apiRoot = Path.GetFullPath(args.Single());
var apiDll = Path.Combine(apiRoot, "src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll");
if (!File.Exists(apiDll)) throw new InvalidOperationException("Build the target API in Release first.");
var connection = Environment.GetEnvironmentVariable("BRANCHFLOW_AUTH_TEST_DB") ??
    @"Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True";
var settings = new SqlConnectionStringBuilder(connection);
if (settings.InitialCatalog != "BranchFlow_Test" ||
    !(settings.DataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
      settings.DataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("Only localhost / BranchFlow_Test is allowed.");

await using var db = new BranchFlowDbContext(
    new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options);
var created = new List<object>();
T Add<T>(T entity) where T : class { db.Add(entity); created.Add(entity); return entity; }
var suffix = Guid.NewGuid().ToString("N")[..12];
var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
var hasher = new PasswordHasher<AppUser>();
var now = DateTime.UtcNow;
Process? host = null;
var hostLogs = new StringBuilder();
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:19119"), Timeout = TimeSpan.FromSeconds(10) };
var passed = new List<string>();
var failed = new List<string>();

async Task StopHostAsync()
{
    if (host is null) return;
    if (!host.HasExited) { host.Kill(true); await host.WaitForExitAsync(); }
    host.Dispose();
    host = null;
}

async Task StartHostAsync()
{
    await StopHostAsync();
    var start = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = apiRoot,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    foreach (var arg in new[] { apiDll, "--urls", "http://localhost:19119", "--contentRoot",
        Path.Combine(apiRoot, "src/BranchFlow.Api") }) start.ArgumentList.Add(arg);
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    start.Environment["ConnectionStrings__BranchFlowDb"] = connection;
    start.Environment["Jwt__Key"] = key;
    start.Environment["Jwt__Issuer"] = "AuthSmoke";
    start.Environment["Jwt__Audience"] = "AuthSmoke";
    hostLogs.Clear();
    host = Process.Start(start) ?? throw new InvalidOperationException("Could not start API host.");
    host.OutputDataReceived += (_, e) => { lock (hostLogs) hostLogs.AppendLine(e.Data); };
    host.ErrorDataReceived += (_, e) => { lock (hostLogs) hostLogs.AppendLine(e.Data); };
    host.BeginOutputReadLine();
    host.BeginErrorReadLine();
    for (var i = 0; i < 100; i++)
    {
        if (host.HasExited) throw new InvalidOperationException("API host exited before it was ready.");
        try { using var probe = await http.GetAsync("/api/auth/me"); return; }
        catch (HttpRequestException) { await Task.Delay(100); }
    }
    throw new InvalidOperationException("API host did not become ready.");
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

async Task<(int Status, JsonElement? Json, string? Media, bool NoStore, string? RetryAfter)> SendAsync(
    HttpMethod method, string path, object? body = null, string? access = null)
{
    using var request = new HttpRequestMessage(method, path);
    if (body is not null) request.Content = JsonContent.Create(body);
    if (access is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
    using var response = await http.SendAsync(request);
    var raw = await response.Content.ReadAsStringAsync();
    JsonElement? json = null;
    if (!string.IsNullOrWhiteSpace(raw))
    {
        using var document = JsonDocument.Parse(raw);
        json = document.RootElement.Clone();
    }
    return ((int)response.StatusCode, json, response.Content.Headers.ContentType?.MediaType,
        response.Headers.CacheControl?.NoStore == true,
        response.Headers.TryGetValues("Retry-After", out var values) ? values.SingleOrDefault() : null);
}

void ExpectProblem((int Status, JsonElement? Json, string? Media, bool NoStore, string? RetryAfter) result, int status)
{
    Require(result.Status == status, $"Expected HTTP {status}, got {result.Status}.");
    Require(result.Media == "application/problem+json", "Expected application/problem+json.");
    Require(result.Json is { } json && json.GetProperty("status").GetInt32() == status &&
        json.TryGetProperty("traceId", out _), "ProblemDetails status/traceId missing.");
}

async Task<(string Access, string Refresh, DateTimeOffset AccessExpiry, DateTimeOffset RefreshExpiry)> LoginAsync(
    AppUser user)
{
    var result = await SendAsync(HttpMethod.Post, "/api/auth/login", new { username = user.Username, password });
    Require(result.Status == 200, $"Valid login got HTTP {result.Status}.");
    Require(result.NoStore, "Login response missing no-store.");
    var json = result.Json ?? throw new InvalidOperationException("Login body missing.");
    var access = json.GetProperty("accessToken").GetString() ?? "";
    var refresh = json.GetProperty("refreshToken").GetString() ?? "";
    Require(access.Length > 20 && refresh.Length > 20 && access != refresh, "Token pair missing.");
    Require(!json.TryGetProperty("password", out _) && !json.TryGetProperty("passwordHash", out _) &&
        !json.TryGetProperty("key", out _), "Secret field exposed in login body.");
    return (access, refresh, json.GetProperty("accessTokenExpiresAt").GetDateTimeOffset(),
        json.GetProperty("refreshTokenExpiresAt").GetDateTimeOffset());
}

string ExpiredAccess(string access)
{
    var old = new JwtSecurityTokenHandler().ReadJwtToken(access);
    var claims = old.Claims.Where(c => c.Type is "sub" or "sid" or "role").ToArray();
    return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("AuthSmoke", "AuthSmoke", claims,
        DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(-1),
        new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
}

async Task RunAsync(string caseId, Func<Task> body)
{
    try
    {
        await StartHostAsync();
        await body();
        passed.Add(caseId);
        Console.WriteLine($"PASS {caseId}");
    }
    catch (Exception error)
    {
        failed.Add(caseId);
        Console.WriteLine($"FAIL {caseId}: {error.GetType().Name}: {error.Message}");
    }
    finally { await StopHostAsync(); }
}

var users = new List<AppUser>();
try
{
    var roles = await db.Roles.AsNoTracking().Where(r => !r.Deleted)
        .ToDictionaryAsync(r => r.Code, r => r.Id);
    AppUser User(string role, string name, bool active = true)
    {
        var user = Add(new AppUser { RoleId = roles[role], Username = $"authqa.{name}.{suffix}",
            FullName = $"Auth QA {name}", IsActive = active });
        user.PasswordHash = hasher.HashPassword(user, password);
        users.Add(user);
        return user;
    }
    var employeeA = User("EMPLOYEE", "a");
    var employeeB = User("EMPLOYEE", "b");
    var owner = User("OWNER", "owner");
    var systemAdmin = User("SYSTEM_ADMIN", "system");
    var branchAdmin = User("BRANCH_ADMIN", "admin");
    var locked = User("EMPLOYEE", "locked", false);
    var branchA = Add(new Branch { Code = "AQ" + suffix, Name = "Auth QA A", Address = "Local test", IsActive = true });
    var branchB = Add(new Branch { Code = "BQ" + suffix, Name = "Auth QA B", Address = "Local test", IsActive = true });
    var profileA = Add(new EmployeeProfile { User = employeeA, EmployeeCode = "AQ" + suffix,
        HireDate = DateOnly.FromDateTime(now) });
    var profileB = Add(new EmployeeProfile { User = employeeB, EmployeeCode = "BQ" + suffix,
        HireDate = DateOnly.FromDateTime(now) });
    Add(new EmployeeBranchAssignment { EmployeeProfile = profileA, Branch = branchA,
        AssignedByUser = owner, EffectiveFrom = now.AddDays(-1) });
    Add(new EmployeeBranchAssignment { EmployeeProfile = profileB, Branch = branchB,
        AssignedByUser = owner, EffectiveFrom = now.AddDays(-1) });
    Add(new AdminBranchAssignment { AdminUser = branchAdmin, Branch = branchA,
        AssignedByUser = owner, EffectiveFrom = now.AddDays(-1) });
    await db.SaveChangesAsync();

    async Task<int> SessionCountAsync() => await db.AuthSessions.AsNoTracking()
        .CountAsync(s => s.UserId == employeeA.Id);

    await RunAsync("QA-S4-001", async () =>
    {
        var before = await SessionCountAsync();
        var pair = await LoginAsync(employeeA);
        Require(pair.AccessExpiry > DateTimeOffset.UtcNow && pair.RefreshExpiry > pair.AccessExpiry,
            "Expiry values invalid.");
        Require(await SessionCountAsync() == before + 1, "Login did not persist one session.");
    });

    await RunAsync("QA-S4-002", async () =>
    {
        var before = await SessionCountAsync();
        var wrong = await SendAsync(HttpMethod.Post, "/api/auth/login",
            new { username = employeeA.Username, password = "wrong" });
        var missing = await SendAsync(HttpMethod.Post, "/api/auth/login",
            new { username = "no-such-user-" + suffix, password });
        ExpectProblem(wrong, 401); ExpectProblem(missing, 401);
        Require(wrong.Json!.Value.GetProperty("detail").GetString() ==
            missing.Json!.Value.GetProperty("detail").GetString(), "Auth error leaks account existence.");
        Require(wrong.NoStore && missing.NoStore && await SessionCountAsync() == before,
            "Invalid login changed sessions or cache policy.");
    });

    await RunAsync("QA-S4-003", async () =>
    {
        var result = await SendAsync(HttpMethod.Post, "/api/auth/login",
            new { username = locked.Username, password });
        ExpectProblem(result, 401);
        Require(result.NoStore, "Locked login response missing no-store.");
    });

    await RunAsync("QA-S4-004", async () =>
    {
        var before = await SessionCountAsync();
        foreach (var body in new object[] { new { password }, new { username = employeeA.Username },
            new { username = "   ", password } })
        {
            var result = await SendAsync(HttpMethod.Post, "/api/auth/login", body);
            ExpectProblem(result, 400);
            Require(result.NoStore, "Validation response missing no-store.");
        }
        Require(await SessionCountAsync() == before, "Invalid login created a session.");
    });

    await RunAsync("QA-S4-006", async () =>
    {
        var pair = await LoginAsync(employeeA);
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me"), 401);
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: "malformed"), 401);
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: ExpiredAccess(pair.Access)), 401);
    });

    await RunAsync("QA-S4-007", async () =>
    {
        var old = await LoginAsync(employeeA);
        var result = await SendAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = old.Refresh });
        Require(result.Status == 200 && result.NoStore, "Refresh did not return 200/no-store.");
        var json = result.Json!.Value;
        var access = json.GetProperty("accessToken").GetString() ?? "";
        var refresh = json.GetProperty("refreshToken").GetString() ?? "";
        Require(access.Length > 20 && refresh.Length > 20 && refresh != old.Refresh,
            "Refresh did not rotate token pair.");
        Require(json.GetProperty("refreshTokenExpiresAt").GetDateTimeOffset() <= old.RefreshExpiry &&
            json.GetProperty("accessTokenExpiresAt").GetDateTimeOffset() <= old.RefreshExpiry,
            "Refresh exceeded absolute session expiry.");
        Require((await SendAsync(HttpMethod.Get, "/api/auth/me", access: access)).Status == 200,
            "New access token rejected.");
    });

    await RunAsync("QA-S4-008", async () =>
    {
        var old = await LoginAsync(employeeA);
        var rotated = await SendAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = old.Refresh });
        Require(rotated.Status == 200, "First refresh failed.");
        var reused = await SendAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = old.Refresh });
        ExpectProblem(reused, 401);
        var newAccess = rotated.Json!.Value.GetProperty("accessToken").GetString() ?? "";
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: newAccess), 401);
    });

    await RunAsync("QA-S4-009", async () =>
    {
        var old = await LoginAsync(employeeA);
        var sessionId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(old.Access)
            .Claims.Single(c => c.Type == "sid").Value);
        var results = await Task.WhenAll(
            SendAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = old.Refresh }),
            SendAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = old.Refresh }));
        Require(results.Count(r => r.Status == 200) == 1 && results.Count(r => r.Status == 401) == 1,
            $"Concurrent refresh statuses were {results[0].Status}/{results[1].Status}.");
        Require(await db.RefreshTokens.AsNoTracking().CountAsync(t => t.SessionId == sessionId) == 2,
            "Concurrent refresh created duplicate descendants.");
    });

    await RunAsync("QA-S4-010", async () =>
    {
        var pair = await LoginAsync(employeeA);
        var sessionId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(pair.Access)
            .Claims.Single(c => c.Type == "sid").Value);
        var logout = await SendAsync(HttpMethod.Post, "/api/auth/logout", access: pair.Access);
        Require(logout.Status == 204 && logout.NoStore, "Logout did not return 204/no-store.");
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: pair.Access), 401);
        ExpectProblem(await SendAsync(HttpMethod.Post, "/api/auth/refresh",
            new { refreshToken = pair.Refresh }), 401);
        Require((await db.AuthSessions.AsNoTracking().SingleAsync(s => s.Id == sessionId)).RevokedAt is not null,
            "Logout did not revoke SQL session.");
    });

    await RunAsync("QA-S4-011", async () =>
    {
        var first = await LoginAsync(employeeA);
        var second = await LoginAsync(employeeA);
        Require((await SendAsync(HttpMethod.Post, "/api/auth/logout", access: first.Access)).Status == 204,
            "First session logout failed.");
        ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: first.Access), 401);
        Require((await SendAsync(HttpMethod.Get, "/api/auth/me", access: second.Access)).Status == 200,
            "Second session was revoked by first logout.");
        Require((await SendAsync(HttpMethod.Post, "/api/auth/refresh",
            new { refreshToken = second.Refresh })).Status == 200, "Second session refresh failed.");
    });

    await RunAsync("QA-S4-012", async () =>
    {
        var pair = await LoginAsync(employeeA);
        employeeA.IsActive = false;
        await db.SaveChangesAsync();
        try
        {
            ExpectProblem(await SendAsync(HttpMethod.Get, "/api/auth/me", access: pair.Access), 401);
            ExpectProblem(await SendAsync(HttpMethod.Post, "/api/auth/refresh",
                new { refreshToken = pair.Refresh }), 401);
        }
        finally { employeeA.IsActive = true; await db.SaveChangesAsync(); }
    });

    await RunAsync("QA-S4-014", async () =>
    {
        foreach (var (user, role, expectedBranches) in new[]
        {
            (owner, "OWNER", new[] { branchA.Id, branchB.Id }),
            (systemAdmin, "SYSTEM_ADMIN", new[] { branchA.Id, branchB.Id }),
            (branchAdmin, "BRANCH_ADMIN", new[] { branchA.Id }),
            (employeeA, "EMPLOYEE", new[] { branchA.Id })
        })
        {
            var pair = await LoginAsync(user);
            var me = await SendAsync(HttpMethod.Get, "/api/auth/me", access: pair.Access);
            Require(me.Status == 200 && me.Json!.Value.GetProperty("userId").GetInt32() == user.Id &&
                me.Json.Value.GetProperty("role").GetString() == role &&
                !me.Json.Value.TryGetProperty("passwordHash", out _), "/me identity or secret fields incorrect.");
            var branches = await SendAsync(HttpMethod.Get, "/api/branches/accessible", access: pair.Access);
            Require(branches.Status == 200 && branches.Json is { } json && json.ValueKind == JsonValueKind.Array,
                "Accessible branches response invalid.");
            var ids = branches.Json!.Value.EnumerateArray().Select(b => b.GetProperty("id").GetInt32()).ToHashSet();
            Require(expectedBranches.All(ids.Contains) &&
                (role is "EMPLOYEE" or "BRANCH_ADMIN" ? ids.Count == expectedBranches.Length : true),
                "Accessible branch scope incorrect.");
        }
    });

    await RunAsync("QA-S4-005", async () =>
    {
        for (var i = 0; i < 5; i++)
            ExpectProblem(await SendAsync(HttpMethod.Post, "/api/auth/login",
                new { username = employeeA.Username, password = "wrong" }), 401);
        var limited = await SendAsync(HttpMethod.Post, "/api/auth/login",
            new { username = employeeA.Username, password = "wrong" });
        Require(limited.Status == 429 && limited.RetryAfter == "60" && limited.NoStore,
            "Sixth invalid login did not return 429/Retry-After/no-store.");
    });

    Console.WriteLine($"Auth SQL/HTTP smoke: {passed.Count} catalog cases passed, {failed.Count} failed.");
    if (failed.Count > 0) Environment.ExitCode = 1;
}
finally
{
    await StopHostAsync();
    var userIds = users.Where(u => u.Id > 0).Select(u => u.Id).ToArray();
    if (userIds.Length > 0)
    {
        await db.RefreshTokens.Where(t => userIds.Contains(t.Session.UserId)).ExecuteDeleteAsync();
        await db.AuthSessions.Where(s => userIds.Contains(s.UserId)).ExecuteDeleteAsync();
    }
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
