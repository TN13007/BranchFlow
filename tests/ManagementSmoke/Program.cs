using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Local authorization/SQL harness. Shared demo passwords are never read or changed.
var root = Path.GetFullPath(args.Single());
var apiDll = Path.Combine(root, "src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll");
if (!File.Exists(apiDll)) throw new InvalidOperationException("Build the API in Release first.");
var connection = Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set BRANCHFLOW_TEST_CONNECTION_STRING.");
var settings = new SqlConnectionStringBuilder(connection);
if (!settings.InitialCatalog.Equals("BranchFlow_Test", StringComparison.OrdinalIgnoreCase) ||
    !Regex.IsMatch(settings.DataSource, @"^(localhost|127\.0\.0\.1|\.)(\\[A-Za-z0-9_]+)?$", RegexOptions.IgnoreCase))
    throw new InvalidOperationException("Only local BranchFlow_Test is allowed.");
var options = new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options;
var sessions = new List<Guid>();
var tokens = new Dictionary<string, string>();
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
const string issuer = "BranchFlow.ManagementSmoke";
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
var url = $"https://localhost:{port}";
Process? api = null;
try
{
    await using (var db = new BranchFlowDbContext(options))
    {
        foreach (var pair in new Dictionary<string, string> {
            ["SYSTEM_ADMIN"] = "demo.systemadmin", ["OWNER"] = "demo.owner",
            ["BRANCH_ADMIN_01"] = "demo.branchadmin.01", ["EMPLOYEE_A"] = "demo.employee.a" })
        {
            var user = await db.AppUsers.Include(x => x.Role)
                .SingleAsync(x => x.Username == pair.Value && x.IsActive && !x.Deleted);
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            sessions.Add(id);
            db.AuthSessions.Add(new AuthSession { Id = id, UserId = user.Id, CreatedAt = now, ExpiresAt = now.AddMinutes(15) });
            tokens[pair.Key] = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer, issuer, [new Claim("sub", user.Id.ToString()), new Claim("sid", id.ToString()), new Claim("role", user.Role.Code)],
                now.AddSeconds(-5), now.AddMinutes(10),
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
        }
        await db.SaveChangesAsync();
    }
    var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var arg in new[] { apiDll, "--urls", url, "--contentRoot", Path.Combine(root, "src/BranchFlow.Api") }) start.ArgumentList.Add(arg);
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
    start.Environment["ConnectionStrings__BranchFlowDb"] = connection;
    start.Environment["Jwt__Key"] = key;
    start.Environment["Jwt__Issuer"] = issuer;
    start.Environment["Jwt__Audience"] = issuer;
    start.Environment["Logging__LogLevel__Default"] = "Critical";
    api = Process.Start(start) ?? throw new InvalidOperationException("Cannot start local API.");
    // Drain both pipes; never forward host output that could include request details.
    var stdout = api.StandardOutput.ReadToEndAsync();
    var stderr = api.StandardError.ReadToEndAsync();
    using var client = new HttpClient(new HttpClientHandler {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    }) { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(2) };
    var ready = false;
    for (var i = 0; i < 80; i++)
    {
        if (api.HasExited) throw new InvalidOperationException("Local API exited; ensure a .NET HTTPS development certificate exists.");
        try { using var r = await client.GetAsync("/api/management/branches"); ready = r.StatusCode == HttpStatusCode.Unauthorized; if (ready) break; }
        catch (HttpRequestException) { }
        catch (TaskCanceledException) { }
        await Task.Delay(150);
    }
    if (!ready) throw new InvalidOperationException("Local API did not become ready.");
    var test = new ProcessStartInfo("pwsh") { UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = root };
    foreach (var arg in new[] { "-NoProfile", "-File", Path.Combine(root, "tests/sprint5/ManagementApiSmoke.ps1"),
        "-BaseUrl", url, "-Server", settings.DataSource, "-UseSyntheticSessions" }) test.ArgumentList.Add(arg);
    test.Environment["BRANCHFLOW_MANAGEMENT_SESSION_TOKENS"] = JsonSerializer.Serialize(tokens);
    test.Environment.Remove("BRANCHFLOW_DEMO_PASSWORD");
    using var child = Process.Start(test) ?? throw new InvalidOperationException("Cannot start PowerShell 7.");
    var output = child.StandardOutput.ReadToEndAsync();
    var errors = child.StandardError.ReadToEndAsync();
    await child.WaitForExitAsync();
    Console.Write(await output);
    Console.Error.Write(await errors);
    if (child.ExitCode != 0) throw new InvalidOperationException($"Management smoke failed: exit {child.ExitCode}.");
    Console.WriteLine("PASS local management authorization/SQL; synthetic initial sessions, not shared-password login. Temporary-user password login is real.");
}
finally
{
    if (api is not null) { if (!api.HasExited) { api.Kill(true); await api.WaitForExitAsync(); } api.Dispose(); }
    await using var db = new BranchFlowDbContext(options);
    await db.RefreshTokens.Where(x => sessions.Contains(x.SessionId)).ExecuteDeleteAsync();
    await db.AuthSessions.Where(x => sessions.Contains(x.Id)).ExecuteDeleteAsync();
    Console.WriteLine("CLEANUP owned bootstrap sessions removed; shared users/passwords unchanged.");
}
