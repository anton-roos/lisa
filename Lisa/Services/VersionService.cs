using System.Reflection;

namespace Lisa.Services;

public class VersionService
{
    public string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";
    }
}
