using System.Reflection;

namespace Lisa.Services;

public class VersionService
{
    public static string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";
    }
}