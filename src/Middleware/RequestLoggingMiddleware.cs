using Lisa.Helpers;
using Serilog;
using System.Diagnostics;

namespace Lisa.Middleware;

/// <summary>
/// Middleware to log HTTP requests with structured data for Seq
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var referer = context.Request.Headers.Referer.ToString();

            // Log the request with structured data
            Log.Information("HTTP Request: {Method} {Path} returned {StatusCode} in {DurationMs}ms for {UserId} from {IpAddress}",
                context.Request.Method,
                context.Request.Path + context.Request.QueryString,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                ipAddress);

            // Log additional details for non-successful requests
            if (context.Response.StatusCode >= 400)
            {
                Log.Warning("HTTP Error Response: {Method} {Path} returned {StatusCode} for {UserId} from {IpAddress} - UserAgent: {UserAgent} Referer: {Referer}",
                    context.Request.Method,
                    context.Request.Path + context.Request.QueryString,
                    context.Response.StatusCode,
                    userId,
                    ipAddress,
                    userAgent,
                    referer);
            }

            // Log slow requests (over 5 seconds)
            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                StructuredLogging.LogPerformanceMetric(
                    $"{context.Request.Method} {context.Request.Path}",
                    stopwatch.ElapsedMilliseconds,
                    "SlowRequest",
                    new { UserId = userId, IpAddress = ipAddress, StatusCode = context.Response.StatusCode }
                );
            }
        }
    }
}
