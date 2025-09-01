using Microsoft.VisualStudio.TestPlatform.Utilities;
using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class WebFormsToBlazorTests(ITestOutputHelper output, AspireFixture<WebFormsToBlazorAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<WebFormsToBlazorAppHost>>
{
    [Fact]
    public async Task EnvironmentVariablesSet()
    {
        // Arrange
        var coreEnvVariables = await aspire.GetIncrementalMigrationEnvironmentVariableValuesAsync("core", output);
        var frameworkEnvVariables = await aspire.GetIncrementalMigrationEnvironmentVariableValuesAsync("framework", output);

        // Act
        Assert.Collection(coreEnvVariables,
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__ApiKey", kvp.Key);
                Assert.Equal("{Default-IncrementalMigration-ApiKey.value}", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__RemoteAppUrl", kvp.Key);
                Assert.Equal("{framework.bindings.https.url}", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__Session__IsEnabled", kvp.Key);
                Assert.Equal("True", kvp.Value);
            });

        Assert.Collection(frameworkEnvVariables,
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Proxy__UseForwardedHeaders", kvp.Key);
                Assert.Equal("True", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__ApiKey", kvp.Key);
                Assert.Equal("{Default-IncrementalMigration-ApiKey.value}", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__Session__IsEnabled", kvp.Key);
                Assert.Equal("True", kvp.Value);
            });
    }

    [Fact]
    public async Task CanNavigateToWebFormsPage()
    {
        var app = await aspire.GetApplicationAsync();
        var endpoint = app.GetEndpoint("core");

        await Page.GotoAsync(new Uri(endpoint, "/about").ToString());
        await Page.WaitForSelectorAsync("text=HelloWorld from ASP.NET Core");
        await Page.WaitForSelectorAsync("text=Current path: /about");
    }
}
