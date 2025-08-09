using System.Reflection;

namespace Lisa;

public static class VersionTest
{
    public static void PrintVersionInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        Console.WriteLine("=== Assembly Version Information ===");
        Console.WriteLine($"Assembly Location: {assembly.Location}");
        Console.WriteLine($"Assembly Full Name: {assembly.FullName}");
        
        // Check AssemblyVersion
        var version = assembly.GetName().Version;
        Console.WriteLine($"Assembly Version: {version}");
        
        // Check AssemblyInformationalVersion
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        Console.WriteLine($"Informational Version: {informationalVersion}");
        
        // Check AssemblyFileVersion
        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        Console.WriteLine($"File Version: {fileVersion}");
        
        // Check Product
        var productAttribute = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        Console.WriteLine($"Product: {productAttribute}");
        
        // Check Copyright
        var copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        Console.WriteLine($"Copyright: {copyrightAttribute}");
        
        Console.WriteLine("=== End Version Information ===");
    }
}
