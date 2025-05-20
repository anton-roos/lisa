using Hangfire.Dashboard;
using Lisa.Data;

namespace Lisa.Middleware;

/// <summary>
/// Custom authorization filter for securing Hangfire Dashboard.
/// </summary>
public class HangfireAuthorizationFilter(ILogger<HangfireAuthorizationFilter> logger)
    : IDashboardAuthorizationFilter
{
    private static readonly List<string> AllowedRoles = [Roles.SystemAdministrator];

    /// <summary>
    /// Authorizes access to the Hangfire dashboard.
    /// </summary>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            logger.LogWarning("Unauthorized access attempt to Hangfire Dashboard. User not authenticated.");
            return false;
        }

        if (!UserHasAccess(user))
        {
            logger.LogWarning("Unauthorized access attempt by user {User}.", user.Identity?.Name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the user has an allowed role.
    /// </summary>
    private static bool UserHasAccess(System.Security.Claims.ClaimsPrincipal user)
    {
        return AllowedRoles.Any(user.IsInRole);
    }
}
