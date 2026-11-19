using System.IdentityModel.Tokens.Jwt;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Data;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BranchFlow.Tests.Integration;

[Collection("Order SQL integration")]
public sealed class PasswordRecoveryIntegrationTests
{
    private const string Initial = "Initial-Password-2026!";
    private const string Updated = "Updated-Password-2026!";
    private static readonly AuthTokenSettings Settings = new(new string('x', 48), "password-test", "password-test");
    private static readonly PasswordHasher<AppUser> Hasher = new();

    private static BranchFlowDbContext Open()
    {
        var connection = Environment.GetEnvironmentVariable("BRANCHFLOW_TEST_CONNECTION_STRING")!;
        Assert.Equal("BranchFlow_Test", new SqlConnectionStringBuilder(connection).InitialCatalog);
        return new(new DbContextOptionsBuilder<BranchFlowDbContext>().UseSqlServer(connection).Options);
    }
    private static AuthService Auth(BranchFlowDbContext db, TimeProvider? clock = null) => new(
        new AuthRepository(db, clock ?? TimeProvider.System), new UnitOfWork(db), Hasher, Settings, clock ?? TimeProvider.System);
    private static PasswordService Passwords(BranchFlowDbContext db) => new(new PasswordRepository(db), new UnitOfWork(db), Hasher, TimeProvider.System);
    private static Guid Session(TokenPair pair) => Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(pair.AccessToken).Claims.Single(x => x.Type == "sid").Value);
    private static async Task<AppUser> Seed(BranchFlowDbContext db, bool temporary = false)
    {
        var user = new AppUser { RoleId = await db.Roles.Where(x => x.Code == "EMPLOYEE").Select(x => x.Id).SingleAsync(),
            Username = "qa.password." + Guid.NewGuid().ToString("N"), FullName = "Password QA", IsActive = true,
            MustChangePassword = temporary, TemporaryPasswordExpiresAt = temporary ? DateTime.UtcNow.AddHours(24) : null };
        user.PasswordHash = Hasher.HashPassword(user, Initial);
        db.AppUsers.Add(user); await db.SaveChangesAsync(); db.ChangeTracker.Clear(); return user;
    }
    private static async Task Cleanup(BranchFlowDbContext db, int id)
    {
        db.ChangeTracker.Clear();
        await db.PasswordResetRequests.Where(x => x.UserId == id).ExecuteDeleteAsync();
        await db.AuditLogs.Where(x => x.EntityName == "AppUser" && x.EntityId == id).ExecuteDeleteAsync();
        await db.RefreshTokens.Where(x => x.Session.UserId == id).ExecuteDeleteAsync();
        await db.AuthSessions.Where(x => x.UserId == id).ExecuteDeleteAsync();
        await db.AppUsers.Where(x => x.Id == id).ExecuteDeleteAsync();
    }

    [SqlServerFact]
    public async Task ChangeRequiresCurrentPasswordAndConfirmation_RevokesAllSessionsWithoutLeakingSecrets()
    {
        await using var db = Open(); var user = await Seed(db, temporary: true);
        try
        {
            var auth = Auth(db); var service = Passwords(db);
            var a = Assert.IsType<TokenPair>(await auth.LoginAsync(user.Username, Initial));
            var b = Assert.IsType<TokenPair>(await auth.LoginAsync(user.Username, Initial));
            Assert.True((await auth.GetCurrentUserAsync(user.Id))!.MustChangePassword);
            await Assert.ThrowsAsync<ArgumentException>(() => service.ChangeAsync(user.Id, Session(a), new("wrong", Updated, Updated)));
            await Assert.ThrowsAsync<ArgumentException>(() => service.ChangeAsync(user.Id, Session(a), new(Initial, "short", "short")));
            await Assert.ThrowsAsync<ArgumentException>(() => service.ChangeAsync(user.Id, Session(a), new(Initial, Updated, "mismatch")));
            await Assert.ThrowsAsync<ArgumentException>(() => service.ChangeAsync(user.Id, Session(a), new(Initial, Initial, Initial)));
            Assert.True(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(a), "EMPLOYEE"));
            await service.ChangeAsync(user.Id, Session(a), new(Initial, Updated, Updated));
            Assert.False(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(b), "EMPLOYEE"));
            Assert.Null(await auth.RefreshAsync(a.RefreshToken)); Assert.Null(await auth.RefreshAsync(b.RefreshToken));
            Assert.Null(await auth.LoginAsync(user.Username, Initial));
            Assert.NotNull(await auth.LoginAsync(user.Username, Updated));
            Assert.False((await auth.GetCurrentUserAsync(user.Id))!.MustChangePassword);
            var stored = await db.AppUsers.AsNoTracking().SingleAsync(x => x.Id == user.Id);
            Assert.Equal(1, stored.CredentialVersion); Assert.Null(stored.TemporaryPasswordExpiresAt);
            Assert.NotEqual(Updated, stored.PasswordHash);
            var audit = await db.AuditLogs.SingleAsync(x => x.EntityName == "AppUser" && x.EntityId == user.Id && x.Action == "UPDATE");
            Assert.DoesNotContain(Initial, audit.AfterData!); Assert.DoesNotContain(Updated, audit.AfterData!);
            Assert.DoesNotContain(stored.PasswordHash, audit.AfterData!);
        }
        finally { await Cleanup(db, user.Id); }
    }

    [SqlServerFact]
    public async Task RecoveryDoesNotChangeCredentials_AdminMustVerifyIdentity_TemporaryPasswordExpires()
    {
        await using var db = Open(); var user = await Seed(db);
        try
        {
            var admin = await db.AppUsers.Where(x => x.Role.Code == "SYSTEM_ADMIN" && x.IsActive && !x.Deleted).Select(x => x.Id).FirstAsync();
            var owner = await db.AppUsers.Where(x => x.Role.Code == "OWNER" && x.IsActive && !x.Deleted).Select(x => x.Id).FirstAsync();
            var service = Passwords(db); var auth = Auth(db);
            var old = Assert.IsType<TokenPair>(await auth.LoginAsync(user.Username, Initial));
            await service.RequestAsync(user.Username); await service.RequestAsync(user.Username);
            await service.RequestAsync("unknown-" + Guid.NewGuid());
            Assert.Single(await db.PasswordResetRequests.Where(x => x.UserId == user.Id && x.ResolvedAt == null).ToListAsync());
            Assert.NotNull(await auth.LoginAsync(user.Username, Initial));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ResetAsync(user.Id, user.Id, new(true, "Forgot password")));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ResetAsync(admin, owner, new(true, "Forgot password")));
            await Assert.ThrowsAsync<ArgumentException>(() => service.ResetAsync(admin, user.Id, new(false, "Forgot password")));
            var issued = await service.ResetAsync(admin, user.Id, new(true, "Forgot password"));
            Assert.InRange(issued.ExpiresAt - DateTimeOffset.UtcNow, TimeSpan.FromMinutes(29), TimeSpan.FromMinutes(31));
            Assert.NotEqual(Initial, issued.TemporaryPassword); Assert.NotEqual("Demo@12345", issued.TemporaryPassword);
            Assert.DoesNotContain(await service.PendingAsync(admin), x => x.UserId == user.Id);
            Assert.False(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(old), "EMPLOYEE"));
            Assert.Null(await auth.RefreshAsync(old.RefreshToken)); Assert.Null(await auth.LoginAsync(user.Username, Initial));
            var restricted = Assert.IsType<TokenPair>(await auth.LoginAsync(user.Username, issued.TemporaryPassword));
            Assert.True((await auth.GetCurrentUserAsync(user.Id))!.MustChangePassword);
            var audit = await db.AuditLogs.SingleAsync(x => x.EntityName == "AppUser" && x.EntityId == user.Id && x.Action == "UPDATE");
            Assert.DoesNotContain(issued.TemporaryPassword, audit.AfterData!);
            await db.AppUsers.Where(x => x.Id == user.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.TemporaryPasswordExpiresAt, DateTime.UtcNow.AddSeconds(-1)));
            Assert.Null(await auth.LoginAsync(user.Username, issued.TemporaryPassword));
            Assert.Null(await auth.RefreshAsync(restricted.RefreshToken));
            Assert.False(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(restricted), "EMPLOYEE"));
        }
        finally { await Cleanup(db, user.Id); }
    }

    [SqlServerFact]
    public async Task LateLoginUsingStaleCredentialsCannotBypassReset_EvenWhenSessionIsCreatedAfterReset()
    {
        await using var db = Open(); var user = await Seed(db);
        try
        {
            var repo = new AuthRepository(db, TimeProvider.System);
            var snapshot = await repo.GetActiveUserByUsernameAsync(user.Username);
            var admin = await db.AppUsers.Where(x => x.Role.Code == "SYSTEM_ADMIN" && x.IsActive && !x.Deleted).Select(x => x.Id).FirstAsync();
            await Passwords(db).ResetAsync(admin, user.Id, new(true, "Forgot password"));
            var stale = new Mock<IAuthRepository>(MockBehavior.Strict);
            stale.Setup(x => x.GetActiveUserByUsernameAsync(user.Username, It.IsAny<CancellationToken>())).ReturnsAsync(snapshot);
            stale.Setup(x => x.AddSession(It.IsAny<AuthSession>())).Callback<AuthSession>(repo.AddSession);
            stale.Setup(x => x.AddRefreshToken(It.IsAny<RefreshToken>())).Callback<RefreshToken>(repo.AddRefreshToken);
            var late = await new AuthService(stale.Object, new UnitOfWork(db), Hasher, Settings, TimeProvider.System).LoginAsync(user.Username, Initial);
            Assert.NotNull(late);
            Assert.False(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(late), "EMPLOYEE"));
            Assert.Null(await Auth(db).RefreshAsync(late.RefreshToken));
        }
        finally { await Cleanup(db, user.Id); }
    }

    [SqlServerFact]
    public async Task ConcurrentChangesDoNotOverwriteEachOther_OnlyOneSucceeds()
    {
        await using var db = Open(); var user = await Seed(db);
        try
        {
            var token = (await Auth(db).LoginAsync(user.Username, Initial))!;
            async Task<bool> Change(string newPassword)
            {
                await using var separate = Open();
                try { await Passwords(separate).ChangeAsync(user.Id, Session(token), new(Initial, newPassword, newPassword)); return true; }
                catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or InvalidOperationException) { return false; }
            }
            var results = await Task.WhenAll(Change(Updated), Change("Another-Password-2026!"));
            Assert.Single(results, x => x);
            Assert.Equal(1, await db.AppUsers.Where(x => x.Id == user.Id).Select(x => x.CredentialVersion).SingleAsync());
            Assert.Single(await db.AuditLogs.Where(x => x.EntityName == "AppUser" && x.EntityId == user.Id).ToListAsync());
        }
        finally { await Cleanup(db, user.Id); }
    }

    [SqlServerFact]
    public async Task ConcurrentResetAndRefreshNeverLeaveAnAuthorizedOldSession()
    {
        await using var db = Open(); var user = await Seed(db);
        try
        {
            var admin = await db.AppUsers.Where(x => x.Role.Code == "SYSTEM_ADMIN" && x.IsActive && !x.Deleted).Select(x => x.Id).FirstAsync();
            string password = Initial;
            for (int i = 0; i < 5; i++)
            {
                var token = Assert.IsType<TokenPair>(await Auth(db).LoginAsync(user.Username, password));
                await using var resetDb = Open(); await using var refreshDb = Open();
                var reset = Passwords(resetDb).ResetAsync(admin, user.Id, new(true, "Concurrent reset QA"));
                var refresh = Auth(refreshDb).RefreshAsync(token.RefreshToken);
                await Task.WhenAll(reset, refresh);
                password = (await reset).TemporaryPassword;
                Assert.False(await new MenuAccessRepository(db, TimeProvider.System).IsActiveSessionAsync(user.Id, Session(token), "EMPLOYEE"));
                if (await refresh is { } rotated)
                    Assert.Null(await Auth(db).RefreshAsync(rotated.RefreshToken));
            }
        }
        finally { await Cleanup(db, user.Id); }
    }
}
