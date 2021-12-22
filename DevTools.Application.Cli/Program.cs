using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.RegularExpressions;
using DevTools.Core;
using DevTools.Core.CommandHandlers;

namespace DevTools.Application.Cli;

internal class Program
{
    private const string EnvIncludePreRelease = "DEVTOOLS_NUGET_INCLUDE_PRERELEASE";
    private const string EnvIncludeRelease = "DEVTOOLS_NUGET_INCLUDE_RELEASE";
    private const string EnvNugetSourceName = "DEVTOOLS_NUGET_SOURCE";
    private const string EnvCodeFolder = "DEVTOOLS_CODE_FOLDER";
    private static async Task Main(string[] args) => await CreateCommandBuilder().InvokeAsync(args);

    /// <summary>
    /// Setup the command hierarchy.
    /// </summary>
    /// <returns></returns>
    private static Command CreateCommandBuilder() => new RootCommand
    {
        new Command("nuget")
        {
            new Command("set-to-latest-local")
            {
                new Option<string>("--nuget-source", GetDefaultNugetSource)
                    .SetDescription("The name of the local nuget source. Environment Variable=" + EnvNugetSourceName),
                CreateCodeFolderOption(),
                new Option<bool>("--include-prerelease", GetIncludePreRelease)
                    .SetDescription(
                        "Include PreRelease versions when determining the latest version. Environment Variable=" +
                        EnvIncludePreRelease),
                new Option<bool>("--include-release", GetIncludeRelease)
                    .SetDescription(
                        "Include Release versions when determining the latest version. Environment Variable=" +
                        EnvIncludeRelease),
                new Option<bool>("--watch", () => false)
                    .SetDescription(
                        "The --nuget-source will be monitored for changes. When changes are detected, the --code-folder will be automatically updated.")
            }.SetHandler(CommandHandler.Create(SetToLatestLocal)),
            new Command("set-to-specific-version")
            {
                CreateCodeFolderOption(),
                new Option<string>("--pattern")
                    .SetRequired()
                    .SetDescription("Regex to identify the projects to update."),
                new Option<string>("--version")
                    .SetRequired()
                    .SetDescription("Set all package references, whose name matches --pattern, to use this version.")
            }.SetHandler(CommandHandler.Create(SetToSpecificVersion))
        }
    };

    private static Option<string> CreateCodeFolderOption() =>
        new Option<string>("--code-folder", GetDefaultCodeFolder)
            .SetDescription("The code folder to execute against (recursively). Environment Variable=" +
                            EnvCodeFolder);

    private static bool GetIncludePreRelease() =>
        GetEnvironmentBoolOrDefault(EnvIncludePreRelease, true);

    private static bool GetIncludeRelease() =>
        GetEnvironmentBoolOrDefault(EnvIncludeRelease, true);

    private static string GetDefaultNugetSource() =>
        GetEnvironmentOrDefault(EnvNugetSourceName, "dev-local");

    private static string GetDefaultCodeFolder() =>
        GetEnvironmentOrDefault(EnvCodeFolder, Directory.GetCurrentDirectory());

    private static string GetEnvironmentOrDefault(string environmentVariableName, string defaultValue)
    {
        var env = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(env)
            ? defaultValue
            : env;
    }

    private static bool GetEnvironmentBoolOrDefault(string environmentVariableName, bool defaultValue) =>
        GetEnvironmentOrDefault(environmentVariableName, defaultValue.ToString())
            .Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase);

    private static async Task SetToLatestLocal(string nugetSource, string codeFolder, bool includePreRelease,
        bool includeRelease, bool watch)
    {
        var nugetFolder = NugetUtility.GetSource(nugetSource)?.SourceUri.LocalPath;
        if (nugetFolder == null) throw new DirectoryNotFoundException(nugetFolder);
        
        Console.WriteLine("Nuget Source Name: " + nugetSource);
        PrintFolderName("Nuget Source Folder", nugetFolder);
        PrintFolderName(" Code Folder", codeFolder);
        Console.WriteLine("Include Releases: " + includeRelease);
        Console.WriteLine("Include Pre-Releases: " + includePreRelease);
        Console.WriteLine("Watch: " + watch);
        Console.WriteLine();

        var command = new SetToLatestLocalCommand(nugetSource, codeFolder, includeRelease, includeRelease);
        await SetToLatestLocalCommandHandler.ExecuteAsync(command);
        if (watch)
        {
            StartWatch(command);
        }
    }

    /// <summary>
    /// Starts a file watcher on the NuGet source folder.
    /// </summary>
    /// <param name="command"></param>
    private static void StartWatch(SetToLatestLocalCommand command)
    {
        // HACK : this is all hacked together. things aren't being properly disposed, etc. it's a mess.

        var action = () =>
        {
            // hack
            SetToLatestLocalCommandHandler.ExecuteAsync(command).Wait();
            Console.WriteLine("Press ENTER to stop watching.");
        };
            
        // many files may be updated in the nuget source at once.
        // we don't want to execute on every event.
        // the time buffer detects activity, then waits for the activity to
        // end. when the activity finishes, it will execute the command.
        // Wait for 2 seconds of inactivity before firing the event.
        var timeBuffer = new TimeBuffer(TimeSpan.FromSeconds(2), () => action());
        timeBuffer.Run(CancellationToken.None);
            
        // each time new files are created in the folder,
        // notify the time buffer.
        // the time buffer will fire the action at the appropriate time.
        using var watcher = new FolderWatcher(command.CodeFolder, "*.nupkg",  () =>
        {
            timeBuffer.NotifyActivity();
        });
            
        watcher.Run();
        Console.WriteLine("Press ENTER to stop watching.");
        Console.ReadLine();
        watcher.Dispose();
        Console.WriteLine("done");
    }

    /// <summary>
    /// Sets all package references, where the package name matches the pattern, to a specific version.
    /// </summary>
    /// <param name="codeFolder"></param>
    /// <param name="version"></param>
    /// <param name="pattern"></param>
    private static async Task SetToSpecificVersion(string codeFolder, string version, string pattern)
    {
        PrintFolderName("Code Folder", codeFolder);
        Console.WriteLine("Version: " + version);
        Console.WriteLine("Pattern: " + pattern);

        
        var command = new SetToSpecificVersionCommand(codeFolder, new Regex(pattern), version);
        await SetToSpecificVersionCommandHandler.ExecuteAsync(command);
    }

    private static void PrintFolderName(string parameterName, string folder)
    {
        var exists = Directory.Exists(folder)
            ? " (exists)"
            : " (Error: doesn't exist)";
        Console.WriteLine(parameterName + ": " + folder + exists);
    }
}