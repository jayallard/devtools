using System.Text.RegularExpressions;

namespace DevTools.Core.CommandHandlers;

public static class SetToSpecificVersionCommandHandler
{
    public static Task ExecuteAsync(SetToSpecificVersionCommand command)
    {
        var projectFiles = ProjectUtility.GetProjectFiles(command.CodeFolder).ToList();
        if (!projectFiles.Any())
        {
            throw new InvalidOperationException("No projects in the folder: " + command.CodeFolder);
        }

        var projects = ProjectUtility.GetProjectFiles(command.CodeFolder);
        ProjectUtility.UpdatePackageReferences(projects, Swapper);

        string? Swapper(PackageReference reference)
        {
            return command.PackageNamePattern.IsMatch(reference.Name)
                ? command.Version
                : null;
        }
        return Task.CompletedTask;
    }
}