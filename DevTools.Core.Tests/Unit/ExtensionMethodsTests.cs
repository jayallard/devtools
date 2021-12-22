using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace DevTools.Core.Tests.Unit;

public class ExtensionMethodsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtensionMethodsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void X()
    {
        var x = new NuGetVersion(1, 2, 3, 4, new[] { "pre", "1" }, null);
        x.IsSemVer2.Should().BeTrue();
        x.ToFullString().Should().Be("1.2.3.4-pre.1");
        x.IsLegacyVersion.Should().BeTrue();
        
        var y = new NuGetVersion(1, 2, 3, new[] { "pre", "1" }, null);
        y.IsSemVer2.Should().BeTrue();
        y.ToFullString().Should().Be("1.2.3-pre.1");
        y.IsLegacyVersion.Should().BeFalse();

        SemanticVersion.Parse("1.2.3.4");
    }

    [Fact]
    public void Test()
    {
        var v = new NuGetVersion(1, 1, 1, new[] { "more-than-1", "is-required" }, "boo");
        v.IsSemVer2.Should().BeTrue();
        _testOutputHelper.WriteLine(v.ToFullString());
    }

    [Fact]
    public void SelectMostRecentBasicSort()
    {
        var sorted = new[]
            {
                Parse("blah.0.0.1.nupkg", 2021, 01, 01),
                Parse("blah.0.0.3.nupkg", 2021, 01, 01),
                Parse("blah.0.0.2.nupkg", 2021, 01, 01),
            }.SelectMostRecent()
            .ToList();

        sorted.Count.Should().Be(1);
        sorted.Single().Version.Major.Should().Be(0);
        sorted.Single().Version.Minor.Should().Be(0);
        sorted.Single().Version.Patch.Should().Be(3);
    }

    /// <summary>
    /// Parses the path to create a NugetPackage object.
    /// Then, changes the date.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <returns></returns>
    private static NugetPackage Parse(string path, int year, int month, int day)
    {
        var actual = NugetFileParser.Parse(path);
        return actual with { FileInfo = new NugetFileInfo(actual.FileInfo.Path, new DateTime(year, month, day)) };
    }

    [Fact]
    public void SelectMostRecentPreReleaseByDate()
    {
        var input = new[]
        {
            Parse("blah.0.0.1-preview2.nupkg", 2021, 12, 02),
            Parse("blah.0.0.1-preview3.nupkg", 2021, 12, 03),
            Parse("blah.0.0.1-preview2.nupkg", 2021, 12, 01)
        };

        var sorted = input
            .SelectMostRecent()
            .ToList();

        sorted.Count.Should().Be(1);
        sorted.Single().Version.Major.Should().Be(0);
        sorted.Single().Version.Minor.Should().Be(0);
        sorted.Single().Version.Patch.Should().Be(1);
        sorted.Single().Version.ToFullString().Should().Be("0.0.1-preview3");
    }

    [Fact]
    public void SelectMostRecentPatchBeatsPreRelease()
    {
        var input = new[]
        {
            Parse("blah.0.0.1-preview1.nupkg", 2020, 12, 01),
            Parse("blah.0.0.1-preview2.nupkg", 2021, 12, 02),
            Parse("blah.0.0.2.nupkg", 2021, 12, 02),
        };

        var sorted = input
            .SelectMostRecent()
            .ToList();

        sorted.Count.Should().Be(1);
        sorted.Single().Version.Major.Should().Be(0);
        sorted.Single().Version.Minor.Should().Be(0);
        sorted.Single().Version.Patch.Should().Be(2);
        sorted.Single().Version.ToFullString().Should().Be("0.0.2");
    }

    [Fact]
    public void SelectMostRecentIgnorePreRelease()
    {
        var input = new[]
        {
            Parse("blah.0.0.3-preview1.nupkg", 2020, 12, 01),
            Parse("blah.0.0.3-preview2.nupkg", 2020, 12, 02),
            Parse("blah.0.0.2.nupkg", 2020, 12, 02),
        };

        var sorted = input
            .SelectMostRecent(includePrerelease: false, includeRelease: true)
            .ToList();

        sorted.Count.Should().Be(1);
        sorted.Single().Version.Major.Should().Be(0);
        sorted.Single().Version.Minor.Should().Be(0);
        sorted.Single().Version.Patch.Should().Be(2);
        sorted.Single().Version.ToFullString().Should().Be("0.0.2");
    }

    [Fact]
    public void SelectMostRecentIgnoreRelease()
    {
        var input = new[]
        {
            Parse("blah.0.0.1-preview2.nupkg", 2021, 12, 2),
            Parse("blah.0.0.1-preview1.nupkg", 2021, 12, 1),
            Parse("blah.0.0.2.nupkg", 2021, 12, 02),
        };

        var sorted = input
            .SelectMostRecent(includePrerelease: true, includeRelease: false)
            .ToList();

        sorted.Count.Should().Be(1);
        sorted.Single().Version.Major.Should().Be(0);
        sorted.Single().Version.Minor.Should().Be(0);
        sorted.Single().Version.Patch.Should().Be(1);
        sorted.Single().Version.ToFullString().Should().Be("0.0.1-preview2");
    }
}