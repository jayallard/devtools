using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Xunit;
using Xunit.Abstractions;

namespace DevTools.Core.Tests.Unit;

public class NugetFileParserTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NugetFileParserTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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

    [Fact]
    public void FlattenJsonToEnvironmentVariable()
    {
        var provider = new MyJsonProvider(new JsonConfigurationSource
        {
            Path = @"dev-config.json",
            FileProvider = new PhysicalFileProvider(@"c:\users\jay\projects")
        });
        provider.Load();
        foreach (var (k, v) in provider.Values)
        {
            //_testOutputHelper.WriteLine($"{k} = {v}\n\n");
            Environment.SetEnvironmentVariable(k.Replace(":", "__"), v);
        }

        foreach (var (k, v) in provider.Values)
        {
            var envVariableName = k.Replace(":", "__");
            var value = Environment.GetEnvironmentVariable(envVariableName) ?? "!!! NOT FOUND";
            var length = Math.Min(20, value.Length);
            _testOutputHelper.WriteLine($"{envVariableName}={value}\n\n");
        }
    }

    public class MyJsonProvider : JsonConfigurationProvider
    {
        public MyJsonProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public IDictionary<string, string> Values => Data;
    }
}