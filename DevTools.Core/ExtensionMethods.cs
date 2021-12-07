using System.Xml.Linq;

namespace DevTools.Core;

public static class ExtensionMethods
{
    /// <summary>
    /// Returns the most recent version of each nuget package.
    /// When there are multiple releases (ie: prerelease) with
    /// the same MAJOR.MINOR.PATCH,
    /// then file date is used to determine order.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="includePrerelease"></param>
    /// <param name="includeRelease"></param>
    /// <returns></returns>
    public static IEnumerable<NugetPackage> SelectMostRecent(
        this IEnumerable<NugetPackage> files,
        bool includePrerelease = true,
        bool includeRelease = true) =>
        files
            .Where(p =>
                (p.Version.IsPrerelease && includePrerelease)
                || !p.Version.IsPrerelease && includeRelease
            )
            .GroupBy(p => p.PackageName)
            .Select(g => g
                .OrderByDescending(p => p.Version)
                .ThenByDescending(p => p.FileInfo.CreationTime)
                .First());

    /// <summary>
    /// Throws an exception if the attribute doesn't exist.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static XAttribute AttributeRequired(this XElement element, string name) =>
        element.Attribute(name) ?? throw new ArgumentException("Attribute doesn't exist: " + name + "\n" + element);
}