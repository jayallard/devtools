namespace DevTools.Core.CommandHandlers;

/// <summary>
/// Updates all projects in TargetFolder with the latest preview versions from the NugetFolder.
/// </summary>
public static class SetToLatestLocalCommandHandler
{
    public static Task ExecuteAsync(SetToLatestLocalCommand command, CancellationToken cancellationToken = default)
    {
        FileUtility.EnsureFolderExists("CodeFolder", command.CodeFolder);
        return Task.Run(Run, cancellationToken);
        void Run()
        {
            var projectFiles = ProjectUtility.GetProjectFiles(command.CodeFolder).ToList();
            if (!projectFiles.Any())
            {
                throw new InvalidOperationException("No projects in the folder: " + command.CodeFolder);
            }

            var localNugetPackages = NugetUtility
                .GetNugetPackages(command.NugetSourceName)
                .SelectMostRecent(includePrerelease: command.IncludePreRelease, includeRelease: command.IncludeRelease)
                .ToDictionary(p => p.PackageName);

            ProjectUtility.UpdatePackageReferences(projectFiles, Swapper);
            string? Swapper(PackageReference p)
            {
                var found = localNugetPackages.TryGetValue(p.Name, out var match);
                return found
                    ? match!.Version.ToFullString()
                    : null;
            }
        }
    }
}