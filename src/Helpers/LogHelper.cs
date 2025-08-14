using Serilog;

namespace Lisa.Helpers;

/// <summary>
/// Helper class for structured logging with Seq integration
/// </summary>
public static class LogHelper
{
    /// <summary>
    /// Log user activity with structured data
    /// </summary>
    public static void LogUserActivity(string userId, string activity, object? additionalData = null)
    {
        Log.Information("User activity: {Activity} by {UserId} with data {@AdditionalData}", 
            activity, userId, additionalData);
    }

    /// <summary>
    /// Log database operations
    /// </summary>
    public static void LogDatabaseOperation(string operation, string entityType, object? entityId = null, TimeSpan? duration = null)
    {
        Log.Information("Database operation: {Operation} on {EntityType} {EntityId} took {Duration}ms",
            operation, entityType, entityId, duration?.TotalMilliseconds);
    }

    /// <summary>
    /// Log authentication events
    /// </summary>
    public static void LogAuthentication(string userId, string action, bool success, string? reason = null)
    {
        if (success)
        {
            Log.Information("Authentication success: {Action} for {UserId}", action, userId);
        }
        else
        {
            Log.Warning("Authentication failed: {Action} for {UserId}. Reason: {Reason}", action, userId, reason);
        }
    }

    /// <summary>
    /// Log performance metrics
    /// </summary>
    public static void LogPerformance(string operation, TimeSpan duration, object? metrics = null)
    {
        Log.Information("Performance: {Operation} completed in {Duration}ms with metrics {@Metrics}",
            operation, duration.TotalMilliseconds, metrics);
    }

    /// <summary>
    /// Log business events
    /// </summary>
    public static void LogBusinessEvent(string eventType, object eventData)
    {
        Log.Information("Business event: {EventType} with data {@EventData}", eventType, eventData);
    }

    /// <summary>
    /// Log errors with context
    /// </summary>
    public static void LogError(Exception exception, string context, object? additionalData = null)
    {
        Log.Error(exception, "Error in {Context} with data {@AdditionalData}", context, additionalData);
    }
}
