using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.EnvironmentName != "Development" &&
    builder.Environment.EnvironmentName != "Testing")
{
    throw new InvalidOperationException(
        "DemoSeed chỉ được chạy trong môi trường Development hoặc Testing.");
}

string? connectionString = builder.Configuration.GetConnectionString("BranchFlowDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Thiếu ConnectionStrings:BranchFlowDb. Hãy cấu hình biến môi trường trước khi chạy.");
}

SqlConnectionStringBuilder sqlSettings = new(connectionString);
string dataSource = sqlSettings.DataSource.Trim();
bool isLocalServer =
    dataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
    dataSource.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
    dataSource.Equals(".", StringComparison.OrdinalIgnoreCase) ||
    dataSource.Equals("(local)", StringComparison.OrdinalIgnoreCase) ||
    dataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase) ||
    dataSource.StartsWith(".\\", StringComparison.OrdinalIgnoreCase);

bool isAllowedDatabase =
    sqlSettings.InitialCatalog.Equals("BranchFlow_Dev", StringComparison.OrdinalIgnoreCase) ||
    sqlSettings.InitialCatalog.Equals("BranchFlow_Test", StringComparison.OrdinalIgnoreCase);

if (!isLocalServer || !isAllowedDatabase)
{
    throw new InvalidOperationException(
        "DemoSeed chỉ chấp nhận SQL Server local và database BranchFlow_Dev hoặc BranchFlow_Test.");
}

builder.Services.AddDbContext<BranchFlowDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDemoSeedRepository, DemoSeedRepository>();
builder.Services.AddScoped<IDemoSeedService, DemoSeedService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

await using IHost host = builder.Build();
await using AsyncServiceScope scope = host.Services.CreateAsyncScope();

string? passwordFromEnvironment = Environment.GetEnvironmentVariable("BRANCHFLOW_DEMO_PASSWORD");
string password = string.IsNullOrEmpty(passwordFromEnvironment)
    ? ReadPassword()
    : passwordFromEnvironment;

try
{
    IDemoSeedService seedService = scope.ServiceProvider.GetRequiredService<IDemoSeedService>();
    IAuthRepository authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

    DemoSeedResult seedResult = await seedService.SeedAsync(password);
    await VerifyQueryMatrixAsync(authRepository, seedResult);

    Console.WriteLine();
    Console.WriteLine("Demo data đã sẵn sàng.");
    Console.WriteLine("Mật khẩu nhập ở đầu chương trình chỉ áp dụng cho tài khoản được tạo mới.");
    Console.WriteLine("Seeder không hiển thị hoặc ghi mật khẩu/hash ra màn hình.");
}
finally
{
    Environment.SetEnvironmentVariable("BRANCHFLOW_DEMO_PASSWORD", null);
}

static string ReadPassword()
{
    Console.Write("Nhập mật khẩu cho các tài khoản demo mới (ít nhất 12 ký tự): ");

    StringBuilder password = new();
    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}

static async Task VerifyQueryMatrixAsync(
    IAuthRepository authRepository,
    DemoSeedResult seedResult)
{
    Dictionary<string, string[]> expectedBranches = new()
    {
        ["demo.owner"] = ["BF-DEMO-01", "BF-DEMO-02", "BF-DEMO-03"],
        ["demo.systemadmin"] = ["BF-DEMO-01", "BF-DEMO-02", "BF-DEMO-03"],
        ["demo.branchadmin.01"] = ["BF-DEMO-01"],
        ["demo.branchadmin.02"] = ["BF-DEMO-02"],
        ["demo.branchadmin.03"] = ["BF-DEMO-03"],
        ["demo.employee.a"] = ["BF-DEMO-01"],
        ["demo.employee.b"] = ["BF-DEMO-02"],
        ["demo.employee.locked"] = [],
        ["demo.employee.unassigned"] = []
    };

    foreach (DemoSeedAccountResult account in seedResult.Accounts)
    {
        AuthUserData? activeUser = await authRepository.GetActiveUserByUsernameAsync(account.Username);
        if ((activeUser is not null) != account.IsActive)
        {
            throw new InvalidOperationException(
                $"Truy vấn tài khoản '{account.Username}' không khớp trạng thái mong đợi.");
        }

        if (activeUser is not null && activeUser.RoleCode != account.RoleCode)
        {
            throw new InvalidOperationException(
                $"Role của tài khoản '{account.Username}' không khớp dữ liệu seed.");
        }

        IReadOnlyList<AccessibleBranchData> accessibleBranches =
            await authRepository.GetAccessibleBranchesAsync(account.UserId);

        string[] actualCodes = accessibleBranches
            .Select(branch => branch.Code)
            .ToArray();
        string[] expectedCodes = expectedBranches[account.Username];

        if (!actualCodes.SequenceEqual(expectedCodes))
        {
            throw new InvalidOperationException(
                $"Sai phạm vi chi nhánh của '{account.Username}'. " +
                $"Mong đợi: [{string.Join(", ", expectedCodes)}], " +
                $"thực tế: [{string.Join(", ", actualCodes)}].");
        }

        string accountState = account.IsActive ? "active" : "locked";
        Console.WriteLine(
            $"PASS | {account.Username} | {account.RoleCode} | {accountState} | " +
            $"expected=[{string.Join(", ", expectedCodes)}] " +
            $"actual=[{string.Join(", ", actualCodes)}]");
    }

    Console.WriteLine($"Đã kiểm tra {seedResult.Accounts.Count} tài khoản và phạm vi chi nhánh.");
}
