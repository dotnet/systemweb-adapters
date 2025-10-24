using Projects;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class DependencyInjectionTests(AspireFixture<DependencyInjectionAppHost> aspire) : IClassFixture<AspireFixture<DependencyInjectionAppHost>>
{
    [InlineData("/handler")]
    [InlineData("/mvc")]
    [InlineData("/api")]
    [Theory]
    public async Task CheckFrameworkDI(string path)
    {
        var app = await aspire.GetApplicationAsync();
        using var client = app.CreateHttpClient("framework");
        var response = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        Assert.True(bool.Parse(response));
    }
}

