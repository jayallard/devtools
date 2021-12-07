using FluentAssertions;
using Xunit;

namespace DevTools.Core.Tests.Unit;

public class NugetFileParserTests
{
    [Fact]
    public void ParseFileName()
    {
        // arrange, act
        const string path =
            @"C:\Users\jay\projects\.dev-nuget\PPM.WebApi.Logging.0.0.8-preview202112022113425105fc17f0e2d7d1c5a9855293336144d1076b6b.symbols.nupkg";
        var f = NugetFileParser.Parse(path);

        // assert
        f.Version.Major.Should().Be(0);
        f.Version.Minor.Should().Be(0);
        f.Version.Patch.Should().Be(8);
    }
}