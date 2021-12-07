using System.Collections.Immutable;
using System.Xml.Linq;

namespace DevTools.Core;

public record CSharpProject(XDocument ProjectXml, string Path, ImmutableList<PackageReference> PackageReferences);

public record PackageReference(XAttribute NameAttribute, XAttribute VersionAttribute)
{
    public string Name => NameAttribute.Value;
    public string Version => VersionAttribute.Value;
};

public static class ProjectUtility
{
    // there are things built in to use this, with Roslyn I think.
    // this can be much better... this is a quickie.
    public static CSharpProject GetProjectXml(string projectPath)
    {
        var proj = XDocument.Load(projectPath);
        var packageReferences = proj
            .Descendants("PackageReference")
            .Select(p => new PackageReference(
                p.AttributeRequired("Include"), 
                p.AttributeRequired("Version")))
            .ToImmutableList();
        return new CSharpProject(proj, projectPath, packageReferences);
    }
    
    private static int UpdatePackageReferences(string projectPath, Func<PackageReference, string?> getNewVersion)
    {
        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        var project = GetProjectXml(projectPath);
        var changeCount = 0;
        
        // todo: move behavior into the CSharpProject class.
        foreach (var pref in project.PackageReferences)
        {
            var updatedVersion = getNewVersion(pref);
            if (updatedVersion == null)
            {
                // method didn't specify a new version, so no change
                // Console.WriteLine($"{projectName}: {pref.Name}: No match; Not updating");
                continue;
            }

            if (updatedVersion == pref.Version)
            {
                // method returned the same version, so no change
                // Console.WriteLine($"{projectName}: {pref.Name}: Version hasn't change; Not updating.");
                continue;
            }
            
            changeCount++;
            Console.WriteLine($"{projectName}: {pref.Name}\n\tFrom: {pref.Version}\n\t  To: {updatedVersion}");
            pref.VersionAttribute.SetValue(updatedVersion);
        }
        
        if (changeCount > 0)
        {
            // this results in formatting changes to the csproj.
            // ignoring white space in a git diff should take care of it
            File.WriteAllText(projectPath, project.ProjectXml.ToString());
        }

        return changeCount;
    }

    public static void UpdatePackageReferences(IEnumerable<string> projectPaths, Func<PackageReference, string?> getNewVersion)
    {
        var updateCount = projectPaths.Sum(p => UpdatePackageReferences(p, getNewVersion));
        Console.WriteLine("\nUpdates: " + updateCount);
    }
    
    public static IEnumerable<string> GetProjectFiles(string codeFolder)
    {
        FileUtility.EnsureFolderExists(nameof(codeFolder), codeFolder);
        return Directory.GetFiles(codeFolder, "*.csproj", SearchOption.AllDirectories);
    }
}