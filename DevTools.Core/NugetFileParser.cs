using System.Globalization;
using NuGet.Versioning;

namespace DevTools.Core;

public static class NugetFileParser
{
    /// <summary>
    /// Extracts nuget information from a file path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static NugetPackage Parse(string path)
    {
        // NOTE: this code should be somewhere in an existing nuget library,
        // but i couldn't find it. I found code where it converst a file
        // to a nuget file name, but not the opposite.
        // reinventing the wheel here.
        var fileName = Path.GetFileNameWithoutExtension(path);
        var major = fileName
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .First(f => int.TryParse(f, out _));

        // where the version begins. to the left is the name, to the right is the full version.
        var versionIndex = fileName.IndexOf("." + major + ".", StringComparison.InvariantCultureIgnoreCase) + 1;
        var packageName = fileName[..(versionIndex - 1)];
        var version = fileName[versionIndex..];
        var semVersion = NuGetVersion.Parse(version);

        var fileInfo = new FileInfo(path);
        var nugetFileInfo = new NugetFileInfo(fileInfo.FullName, fileInfo.CreationTimeUtc);
        return new NugetPackage(nugetFileInfo, semVersion, packageName);
    }
}