using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevTools.Core.CommandHandlers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DevTools.Core.Tests.Integration;

public class Playground
{
    // these help with development and manual tests
    // they're not ready for automation; they expect specific things on
    // the host macthing, such as known directories, nuget source names, etc
    
    
    private const string NugetSourceName = @"dev-local";
    private const string CodeFolder = @"C:\Users\jay\projects\service-file-management\src";
    private readonly ITestOutputHelper _testOutputHelper;

    public Playground(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SetProjectsToLatestLocal()
    {
        var request = new SetToLatestLocalCommand(NugetSourceName, CodeFolder, true, true);
        await SetToLatestLocalCommandHandler.ExecuteAsync(request);
    }

    [Fact]
    public async Task SetProjectsToSpecificVersion()
    {
        var pattern = new Regex("PPM\\.SharedKernel");
        var command = new SetToSpecificVersionCommand(CodeFolder, pattern, "0.0.111");
        await SetToSpecificVersionCommandHandler.ExecuteAsync(command);
    }

    [Fact]
    public void GetSources()
    {
        var sources = NugetUtility.GetSources().ToList();
        sources.Should().NotBeEmpty();

        foreach (var s in sources)
        {
            _testOutputHelper.WriteLine(s.ToString());
            _testOutputHelper.WriteLine(s.SourceUri.Scheme);
        }
    }

    [Fact]
    public void GetSource()
    {
        var source = NugetUtility.GetSource("dev-local");
        source.Should().NotBeNull();
        Debug.Assert(source != null);
        source.Name.Should().Be("dev-local");
        source.SourceUri.Scheme.Should().Be("file");
        _testOutputHelper.WriteLine(source?.ToString());
    }

}