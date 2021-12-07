using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace DevTools.Core;

public record NugetFileInfo(string Path, DateTime CreationTime);
public record NugetPackage(NugetFileInfo FileInfo, SemanticVersion Version, string PackageName);
public record NugetSource(string Name, Uri SourceUri,  string SourceFile);
public record SetToLatestLocalCommand(string NugetSourceName, string CodeFolder, bool IncludePreRelease, bool IncludeRelease);
public record SetToSpecificVersionCommand(string CodeFolder, Regex PackageNamePattern, string Version);
