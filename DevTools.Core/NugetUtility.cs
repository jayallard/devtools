using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DevTools.Core;

public static class NugetUtility
{
    public static IEnumerable<NugetPackage> GetPackagesFromSource(string nugetSourceName)
    {
        var source = GetSource(nugetSourceName) ??
                     throw new ArgumentException("Nuget Source doesn't exist: " + nugetSourceName);
        if (source.SourceUri.Scheme != "file")
        {
            throw new ArgumentException("Only files are supported. Unsupported nuget source: " + source);
        }

        return Directory
            .GetFiles(source.SourceUri.LocalPath, "*.nupkg", SearchOption.AllDirectories)

            // ignore symbols
            .Where(file => !file.EndsWith(".symbols.nupkg"))
            .Select(NugetFileParser.Parse);
    }

    public static IEnumerable<NugetSource> GetSources()
    {
        // just using the global nuget config for now. later, check
        // for all the other files. start simple.
        var configFile = GetGlobalNugetConfig();
        if (!File.Exists(configFile))
        {
            throw new FileNotFoundException("Nuget File doesn't exist: " + configFile);
        }

        var doc = XDocument.Load(configFile);
        var results = doc
            ?.Root
            ?.Element("packageSources")
            ?.Elements("add")
            ?.Select(s => new NugetSource(
                s.AttributeRequired("key").Value,
                new Uri(s.AttributeRequired("value").Value),
                configFile)
            );
        return results ?? Array.Empty<NugetSource>();
    }

    private static string GetGlobalNugetConfig()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(folder, "NuGet", "Nuget.Config");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(folder, ".nuget", "NuGet", "NuGet.Config");
        }

        throw new NotImplementedException("This os isn't supported");
    }

    public static NugetSource? GetSource(string sourceName) => GetSources()
        .Single(s => s.Name.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase));
}