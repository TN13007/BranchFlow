using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using BranchFlow.Application.DTOs.Sprint4;
using BranchFlow.Application.Services;
using BranchFlow.Infrastructure.Models;
using BranchFlow.Infrastructure.Repositories;
using BranchFlow.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BranchFlow.Tests.Backend;

public sealed class AuthServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task LoginWithValidCredentialsCreatesSessionAndReturnsExpectedJwt()
    {
        const string password = "BranchFlow-Valid-Password-2026!";
        const int userId = 42;
        const string username = "demo.employee.a";

        PasswordHasher<AppUser> passwordHasher = new();
        AppUser passwordUser = new() { Id = userId, Username = username };
        string passwordHash = passwordHasher.HashPassword(passwordUser, password);

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetActiveUserByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserData(userId, username, passwordHash, "Demo Employee", "EMPLOYEE"));

        AuthSession? savedSession = null;
        RefreshToken? savedRefreshToken = null;

        repository
            .Setup(repo => repo.AddSession(It.IsAny<AuthSession>()))
            .Callback<AuthSession>(session => savedSession = session);
        repository
            .Setup(repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(refreshToken => savedRefreshToken = refreshToken);

        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        AuthService service = CreateService(repository.Object, unitOfWork.Object, passwordHasher);

        TokenPair? result = await service.LoginAsync($"  {username}  ", password);

        Assert.NotNull(result);
        Assert.NotNull(savedSession);
        Assert.NotNull(savedRefreshToken);
        Assert.Equal(userId, savedSession.UserId);
        Assert.Equal(FixedNow.UtcDateTime, savedSession.CreatedAt);
        Assert.Equal(FixedNow.AddDays(7).UtcDateTime, savedSession.ExpiresAt);
        Assert.Equal(savedSession.Id, savedRefreshToken.SessionId);
        Assert.Equal(FixedNow.AddDays(7).UtcDateTime, savedRefreshToken.ExpiresAt);
        Assert.Equal(HashRefreshToken(result.RefreshToken), savedRefreshToken.TokenHash);
        Assert.NotEqual(result.RefreshToken, savedRefreshToken.TokenHash);

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal(userId.ToString(), jwt.Claims.Single(claim => claim.Type == "sub").Value);
        Assert.Equal(savedSession.Id.ToString("D"), jwt.Claims.Single(claim => claim.Type == "sid").Value);
        Assert.Equal("EMPLOYEE", jwt.Claims.Single(claim => claim.Type == "role").Value);
        Assert.Equal("BranchFlow.Api", jwt.Issuer);
        Assert.Contains("BranchFlow.Web", jwt.Audiences);
        Assert.Equal(FixedNow.AddMinutes(15), result.AccessTokenExpiresAt);
        Assert.Equal(FixedNow.AddDays(7), result.RefreshTokenExpiresAt);

        unitOfWork.Verify(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithWrongPasswordDoesNotCreateSession()
    {
        const string username = "demo.employee.a";
        PasswordHasher<AppUser> passwordHasher = new();
        AppUser passwordUser = new() { Id = 42, Username = username };
        string passwordHash = passwordHasher.HashPassword(passwordUser, "Correct-Password-2026!");

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetActiveUserByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserData(42, username, passwordHash, "Demo Employee", "EMPLOYEE"));

        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        AuthService service = CreateService(repository.Object, unitOfWork.Object, passwordHasher);

        TokenPair? result = await service.LoginAsync(username, "Wrong-Password-2026!");

        Assert.Null(result);
        unitOfWork.Verify(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        repository.Verify(repo => repo.AddSession(It.IsAny<AuthSession>()), Times.Never);
    }

    [Fact]
    public async Task RefreshRotatesTokenWithoutExtendingAbsoluteSessionExpiry()
    {
        const string oldRawToken = "old-refresh-token-value";
        DateTime nowUtc = FixedNow.UtcDateTime;
        RefreshToken oldToken = CreateRefreshToken(oldRawToken, nowUtc);
        byte[] expectedRowVersion = oldToken.RowVersion;
        RefreshToken? savedNextToken = null;

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetRefreshTokenByHashAsync(HashRefreshToken(oldRawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);
        repository
            .Setup(repo => repo.TryMarkRefreshTokenUsedAsync(
                oldToken.Id,
                It.Is<byte[]>(value => value.SequenceEqual(expectedRowVersion)),
                nowUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository
            .Setup(repo => repo.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => savedNextToken = token);

        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        AuthService service = CreateService(repository.Object, unitOfWork.Object, new PasswordHasher<AppUser>());

        TokenPair? result = await service.RefreshAsync(oldRawToken);

        Assert.NotNull(result);
        Assert.NotNull(savedNextToken);
        Assert.NotEqual(oldRawToken, result.RefreshToken);
        Assert.Equal(oldToken.SessionId, savedNextToken.SessionId);
        Assert.Equal(HashRefreshToken(result.RefreshToken), savedNextToken.TokenHash);
        Assert.NotEqual(result.RefreshToken, savedNextToken.TokenHash);
        Assert.Equal(oldToken.Session.ExpiresAt, savedNextToken.ExpiresAt);
        Assert.Equal(new DateTimeOffset(oldToken.Session.ExpiresAt, TimeSpan.Zero), result.RefreshTokenExpiresAt);
        Assert.Equal(FixedNow.AddMinutes(15), result.AccessTokenExpiresAt);

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal(oldToken.Session.Id.ToString("D"), jwt.Claims.Single(claim => claim.Type == "sid").Value);

        unitOfWork.Verify(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReusingRefreshTokenRevokesItsSession()
    {
        const string oldRawToken = "already-used-refresh-token";
        RefreshToken oldToken = CreateRefreshToken(oldRawToken, FixedNow.UtcDateTime);
        oldToken.UsedAt = FixedNow.AddMinutes(-1).UtcDateTime;

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetRefreshTokenByHashAsync(HashRefreshToken(oldRawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);
        repository
            .Setup(repo => repo.RevokeSessionAsync(
                oldToken.SessionId,
                FixedNow.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        AuthService service = CreateService(repository.Object, unitOfWork.Object, new PasswordHasher<AppUser>());

        TokenPair? result = await service.RefreshAsync(oldRawToken);

        Assert.Null(result);
        repository.Verify(repo => repo.RevokeSessionAsync(
            oldToken.SessionId,
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LosingConcurrentRefreshAttemptRevokesSessionWhenTokenWasAlreadyUsed()
    {
        const string oldRawToken = "concurrent-refresh-token";
        RefreshToken firstRead = CreateRefreshToken(oldRawToken, FixedNow.UtcDateTime);
        RefreshToken latestRead = CreateRefreshToken(oldRawToken, FixedNow.UtcDateTime);
        latestRead.UsedAt = FixedNow.UtcDateTime;

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .SetupSequence(repo => repo.GetRefreshTokenByHashAsync(HashRefreshToken(oldRawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstRead)
            .ReturnsAsync(latestRead);
        repository
            .Setup(repo => repo.TryMarkRefreshTokenUsedAsync(
                firstRead.Id,
                It.IsAny<byte[]>(),
                FixedNow.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(repo => repo.RevokeSessionAsync(
                latestRead.SessionId,
                FixedNow.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        AuthService service = CreateService(repository.Object, unitOfWork.Object, new PasswordHasher<AppUser>());

        TokenPair? result = await service.RefreshAsync(oldRawToken);

        Assert.Null(result);
        repository.Verify(repo => repo.RevokeSessionAsync(
            latestRead.SessionId,
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LockedUserCannotRefreshAndItsSessionIsRevoked()
    {
        const string oldRawToken = "locked-user-refresh-token";
        RefreshToken oldToken = CreateRefreshToken(oldRawToken, FixedNow.UtcDateTime);
        oldToken.Session.User.IsActive = false;

        Mock<IAuthRepository> repository = new(MockBehavior.Strict);
        repository
            .Setup(repo => repo.GetRefreshTokenByHashAsync(HashRefreshToken(oldRawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);
        repository
            .Setup(repo => repo.RevokeSessionAsync(
                oldToken.SessionId,
                FixedNow.UtcDateTime,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock<IUnitOfWork> unitOfWork = CreateUnitOfWork();
        AuthService service = CreateService(repository.Object, unitOfWork.Object, new PasswordHasher<AppUser>());

        TokenPair? result = await service.RefreshAsync(oldRawToken);

        Assert.Null(result);
        repository.Verify(repo => repo.RevokeSessionAsync(
            oldToken.SessionId,
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        unitOfWork
            .Setup(unit => unit.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork
            .Setup(unit => unit.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return unitOfWork;
    }

    private static AuthService CreateService(
        IAuthRepository repository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AppUser> passwordHasher)
    {
        Mock<IConfiguration> configuration = new(MockBehavior.Strict);
        configuration.SetupGet(value => value["Jwt:Key"]).Returns(new string('k', 48));
        configuration.SetupGet(value => value["Jwt:Issuer"]).Returns("BranchFlow.Api");
        configuration.SetupGet(value => value["Jwt:Audience"]).Returns("BranchFlow.Web");

        return new AuthService(
            repository,
            unitOfWork,
            passwordHasher,
            configuration.Object,
            new FixedTimeProvider(FixedNow));
    }

    private static RefreshToken CreateRefreshToken(string rawToken, DateTime nowUtc)
    {
        Guid sessionId = Guid.NewGuid();
        DateTime sessionExpiry = nowUtc.AddDays(6);

        AppUser user = new()
        {
            Id = 42,
            Username = "demo.employee.a",
            IsActive = true,
            Deleted = false,
            Role = new Role { Code = "EMPLOYEE", Deleted = false }
        };

        AuthSession session = new()
        {
            Id = sessionId,
            UserId = user.Id,
            CreatedAt = nowUtc.AddDays(-1),
            ExpiresAt = sessionExpiry,
            User = user
        };

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            TokenHash = HashRefreshToken(rawToken),
            CreatedAt = nowUtc.AddDays(-1),
            ExpiresAt = sessionExpiry,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8],
            Session = session
        };
    }

    private static string HashRefreshToken(string rawToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
