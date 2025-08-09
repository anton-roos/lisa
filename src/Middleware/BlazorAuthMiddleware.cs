using Lisa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace Lisa.Middleware;

/// <summary>
/// Stores login information securely with expiration timestamps.
/// </summary>
public class SecureLoginInfo
{
    public string Email { get; init; }
    public string Password { get; private set; }
    private string PasswordHash { get; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public SecureLoginInfo(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
        }

        Email = email;
        Password = password;
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password) => BCrypt.Net.BCrypt.Verify(password, PasswordHash);

    public bool IsExpired(TimeSpan expiration) => DateTime.UtcNow - CreatedAt > expiration;
}


/// <summary>
/// Middleware to handle login via temporary keys.
/// </summary>
public class BlazorAuthMiddleware(RequestDelegate next, ILoginStore loginStore)
{
    public async Task Invoke(HttpContext context, SignInManager<User> signInManager)
    {
        if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
        {
            if (!Guid.TryParse(context.Request.Query["key"], out var key) || !loginStore.TryGetLoginInfo(key, out var info))
            {
                context.Response.Redirect("/login-failed");
                return;
            }

            var user = await signInManager.UserManager.FindByEmailAsync(info.Email);
            if (user == null)
            {
                context.Response.Redirect("/login-failed");
                return;
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, info.Password, false);
            loginStore.Remove(key);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                context.Response.Redirect("/");
                return;
            }

            context.Response.Redirect("/login-failed");
            return;
        }
        else if (context.Request.Path == "/logout" && context.Request.Query.ContainsKey("middleware"))
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/");
            return;
        }

        await next.Invoke(context);
    }
}

/// <summary>
/// In-memory store for temporary login information.
/// </summary>
public interface ILoginStore : IDisposable
{
    Guid StoreLoginInfo(string email, string password);
    bool TryGetLoginInfo(Guid key, out SecureLoginInfo loginInfo);
    void Remove(Guid key);
}

/// <summary>
/// In-memory store for temporary login information.
/// </summary>
public class InMemoryLoginStore : ILoginStore
{
    private readonly ConcurrentDictionary<Guid, SecureLoginInfo> _logins = new();
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(5);
    private readonly Timer _cleanupTimer;

    public InMemoryLoginStore()
    {
        _cleanupTimer = new Timer(_ => CleanupExpiredLogins(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public Guid StoreLoginInfo(string email, string password)
    {
        var key = Guid.NewGuid();
        var loginInfo = new SecureLoginInfo(email, password);
        _logins[key] = loginInfo;
        return key;
    }

    public bool TryGetLoginInfo(Guid key, out SecureLoginInfo loginInfo)
    {
        if (_logins.TryGetValue(key, out loginInfo!))
        {
            if (loginInfo.IsExpired(_expiration))
            {
                Remove(key);
                loginInfo = null!;
                return false;
            }
            return true;
        }
        return false;
    }

    public void Remove(Guid key)
    {
        _logins.TryRemove(key, out _);
    }

    private void CleanupExpiredLogins()
    {
        foreach (var key in _logins.Keys.ToArray())
        {
            if (_logins.TryGetValue(key, out var info) && info.IsExpired(_expiration))
            {
                Remove(key);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
        _logins.Clear();
        GC.SuppressFinalize(this);
    }
}
