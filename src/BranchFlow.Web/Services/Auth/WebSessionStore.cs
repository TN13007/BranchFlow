using System.Collections.Concurrent;
using System.Security.Cryptography;
using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Web.Services.Auth;

public sealed class WebSession(CurrentUser user, TokenPair tokens, DateTimeOffset expiresAt)
{
    public CurrentUser User { get; } = user;
    public TokenPair Tokens { get; set; } = tokens;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
    public SemaphoreSlim Gate { get; } = new(1, 1);
    public bool Revoked { get; set; }
}

// Singleton chứa nhiều phiên theo khóa ngẫu nhiên; không có token dùng chung người dùng.
public sealed class WebSessionStore(TimeProvider clock) : BackgroundService
{
    public const string SessionClaim = "web_session";
    private readonly ConcurrentDictionary<string, WebSession> sessions = new();

    public string Add(CurrentUser user, TokenPair tokens)
    {
        if (!ValidTokens(tokens) || user.UserId <= 0 || string.IsNullOrWhiteSpace(user.Username) ||
            user.Role is not ("OWNER" or "SYSTEM_ADMIN" or "BRANCH_ADMIN" or "EMPLOYEE"))
            throw new InvalidOperationException("Phản hồi xác thực không hợp lệ.");
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var maxExpiry = clock.GetUtcNow().AddDays(7);
        sessions[key] = new WebSession(user, tokens, tokens.RefreshTokenExpiresAt < maxExpiry ? tokens.RefreshTokenExpiresAt : maxExpiry);
        return key;
    }

    public bool ValidTokens(TokenPair tokens) => !string.IsNullOrWhiteSpace(tokens.AccessToken) &&
        !string.IsNullOrWhiteSpace(tokens.RefreshToken) && tokens.AccessTokenExpiresAt > clock.GetUtcNow() &&
        tokens.RefreshTokenExpiresAt > clock.GetUtcNow() && tokens.AccessTokenExpiresAt <= tokens.RefreshTokenExpiresAt;

    public WebSession? Find(string? key)
    {
        if (key is null || !sessions.TryGetValue(key, out var session)) return null;
        if (session.Revoked || session.ExpiresAt <= clock.GetUtcNow()) { Remove(key); return null; }
        return session;
    }

    public void Remove(string key)
    {
        if (sessions.TryRemove(key, out var session)) session.Revoked = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1), clock);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                foreach (var key in sessions.Keys) _ = Find(key);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
