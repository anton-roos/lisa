using Hangfire.Dashboard;
using Lisa.Data;

namespace Lisa.Middleware;
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        // Allow access only if the user is authenticated and in the "Admin" role
        return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole(Roles.SystemAdministrator);
    }
}