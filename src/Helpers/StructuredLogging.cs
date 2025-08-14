using Serilog;

namespace Lisa.Helpers;

/// <summary>
/// Helper class for structured logging with Seq integration
/// </summary>
public static class StructuredLogging
{
    /// <summary>
    /// Log user activity with structured data
    /// </summary>
    public static void LogUserActivity(string action, string userId, string? details = null, object? additionalData = null)
    {
        Log.Information("User Activity: {Action} by {UserId} - {Details} {@AdditionalData}", 
            action, userId, details, additionalData);
    }

    /// <summary>
    /// Log database operations with performance metrics
    /// </summary>
    public static void LogDatabaseOperation(string operation, string entity, long executionTimeMs, object? parameters = null)
    {
        Log.Information("Database Operation: {Operation} on {Entity} completed in {ExecutionTimeMs}ms {@Parameters}", 
            operation, entity, executionTimeMs, parameters);
    }

    /// <summary>
    /// Log business events with context
    /// </summary>
    public static void LogBusinessEvent(string eventName, string context, object? eventData = null)
    {
        Log.Information("Business Event: {EventName} in {Context} {@EventData}", 
            eventName, context, eventData);
    }

    /// <summary>
    /// Log security events
    /// </summary>
    public static void LogSecurityEvent(string eventType, string userId, string ipAddress, bool success, string? reason = null)
    {
        Log.Warning("Security Event: {EventType} for {UserId} from {IpAddress} - Success: {Success} Reason: {Reason}", 
            eventType, userId, ipAddress, success, reason);
    }

    /// <summary>
    /// Log performance metrics
    /// </summary>
    public static void LogPerformanceMetric(string operation, long durationMs, string? category = null, object? metrics = null)
    {
        Log.Information("Performance Metric: {Operation} took {DurationMs}ms Category: {Category} {@Metrics}", 
            operation, durationMs, category, metrics);
    }

    /// <summary>
    /// Log application errors with context
    /// </summary>
    public static void LogError(Exception exception, string context, object? additionalData = null)
    {
        Log.Error(exception, "Error in {Context} {@AdditionalData}", context, additionalData);
    }

    /// <summary>
    /// Log API requests with timing
    /// </summary>
    public static void LogApiRequest(string method, string path, int statusCode, long durationMs, string? userId = null)
    {
        Log.Information("API Request: {Method} {Path} returned {StatusCode} in {DurationMs}ms for user {UserId}", 
            method, path, statusCode, durationMs, userId);
    }
}
