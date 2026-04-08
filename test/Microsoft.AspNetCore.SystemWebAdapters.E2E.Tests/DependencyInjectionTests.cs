using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class DependencyInjectionTests(AspireFixture<DependencyInjectionAppHost> aspire, ITestOutputHelper output) : IClassFixture<AspireFixture<DependencyInjectionAppHost>>
{
    [InlineData("/handler")]
    [InlineData("/mvc")]
    [InlineData("/api")]
    [InlineData("/webforms.aspx")]
    [WindowsOnlyTheory]
    public async Task CheckFrameworkDI(string path)
    {
        using var scope = await aspire.GetApplicationScopeAsync(output);
        using var client = scope.App.CreateHttpClient("framework");
        var response = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        Assert.True(bool.Parse(response));
    }
}

