namespace Lisa.Helpers;

public static class TimeHelpers
{
    public static DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }

    public static DateTime GetCurrentTimeInUtc()
    {
        return DateTime.UtcNow;
    }

    public static DateTime GetCurrentTimeInLocal()
    {
        return DateTime.Now;
    }

    public static DateTime ConvertToUtc(DateTime localTime)
    {
        return localTime.ToUniversalTime();
    }

    public static DateTime ConvertToLocal(DateTime utcTime)
    {
        return utcTime.ToLocalTime();
    }
}