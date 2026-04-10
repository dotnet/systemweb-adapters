using System.Net.Http.Json;
using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class AppConfigTests(AspireFixture<AppConfigAppHost> aspire, ITestOutputHelper output) : IClassFixture<AspireFixture<AppConfigAppHost>>
{
    [WindowsOnlyFact]
    public async Task CoreConfigurationIsConfigured()
    {
        using var scope = aspire.GetApplicationScope(output);
        using var client = scope.App.CreateHttpClient("core");
        var response = await client.GetFromJsonAsync<ConfigResult>("/");

        Assert.NotNull(response);
        Assert.Equal("appsettings.json", response.Setting1);
        Assert.Null(response.Setting2);
        Assert.Equal("appsettings.json", response.ConnStr1);
        Assert.Null(response.ConnStr2);
    }

    [WindowsOnlyFact]
    public async Task FrameworkConfigurationIsConfigured()
    {
        using var scope = aspire.GetApplicationScope(output);
        using var client = scope.App.CreateHttpClient("framework");
        var response = await client.GetFromJsonAsync<ConfigResult>("/");

        Assert.NotNull(response);
        Assert.Equal("appsettings.json", response.Setting1);
        Assert.Equal("web.config", response.Setting2);
        Assert.Equal("appsettings.json", response.ConnStr1);
        Assert.Equal("web.config", response.ConnStr2);
    }

    private sealed class ConfigResult
    {
        public string? Setting1 { get; set; }

        public string? Setting2 { get; set; }

        public string? ConnStr1 { get; set; }

        public string? ConnStr2 { get; set; }
    }
}

