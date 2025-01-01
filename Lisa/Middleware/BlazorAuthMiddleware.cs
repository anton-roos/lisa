using System.Collections.Concurrent;
using Lisa.Data;
using Microsoft.AspNetCore.Identity;

namespace Lisa.Middleware;

public class LoginInfo
{
    public string? Email { get; set; }

    public string? Password { get; set; }
}

public class BlazorAuthMiddleware(RequestDelegate next)
{
    public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
        = new ConcurrentDictionary<Guid, LoginInfo>();

    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context, SignInManager<User> signInManager)
    {
        if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
        {
            var key = Guid.Parse(context.Request.Query["key"]!);
            var info = Logins[key];

            var result = await signInManager.PasswordSignInAsync(info.Email!, info.Password!, false, lockoutOnFailure: true);
            info.Password = null;
            if (result.Succeeded)
            {
                Logins.Remove(key);
                context.Response.Redirect("/");
                return;
            }
            else if (result.RequiresTwoFactor)
            {
                context.Response.Redirect("/login-with-2fa/" + key);
                return;
            }
            else
            {
                context.Response.Redirect("/login-failed");
                return;
            }
        }
        else if (context.Request.Path == "/logout" && context.Request.Query.ContainsKey("middleware"))
        {

            await signInManager.SignOutAsync();
            context.Response.Redirect("/");
            return;
        }
        else
        {
            await _next.Invoke(context);
        }
    }
}