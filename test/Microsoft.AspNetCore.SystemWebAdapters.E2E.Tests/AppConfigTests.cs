using System.Net.Http.Json;
using Projects;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class AppConfigTests(AspireFixture<AppConfigAppHost> aspire) : IClassFixture<AspireFixture<AppConfigAppHost>>
{
    [Fact]
    public async Task ConfigurationIsConfigured()
    {
        var app = await aspire.GetApplicationAsync();
        using var client = app.CreateHttpClient("config");
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

