using System.Globalization;
using Aspire.Hosting;
using Microsoft.Playwright.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class RemoteAuthFormsTests(ITestOutputHelper output, AspireFixture<Projects.AuthRemoteFormsAuthAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<Projects.AuthRemoteFormsAuthAppHost>>
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
                Assert.Equal("IncrementalMigration__Default__Remote__Authentication__IsDefaultScheme", kvp.Key);
                Assert.Equal("True", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__Authentication__IsEnabled", kvp.Key);
                Assert.Equal("True", kvp.Value);
            },
            kvp =>
            {
                Assert.Equal("IncrementalMigration__Default__Remote__RemoteAppUrl", kvp.Key);
                Assert.Equal("{framework.bindings.https.url}", kvp.Value);
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
                Assert.Equal("IncrementalMigration__Default__Remote__Authentication__IsEnabled", kvp.Key);
                Assert.Equal("True", kvp.Value);
            });
    }

    [Fact]
    public async Task CoreAppCanLogout()
    {
        var username = "User1";

        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();
        var coreAppEndpoint = await GetAspNetCoreEndpoint();

        await Page.GotoAsync(frameworkAppEndpoint);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();

        // Login
        await Page.Locator("a:has-text(\"Log In\")").ClickAsync();
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$UserName\"]").FillAsync(username);
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$Password\"]").FillAsync("PasswordA");
        await Page.Locator(@"input:has-text(""Login"")").ClickAsync();
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Make sure core app also logged in
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator($"text=Hello {username}!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator($"text=You have been logged out")).ToBeVisibleAsync();

        // Note: Logout on core app doesn't logout framework app
        //await Page.GotoAsync(FrameworkAppUrl);
        //await Expect(Page.Locator(@"text=Login")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FrameworkCanLogoutBothApps()
    {
        var username = "User1";

        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();
        var coreAppEndpoint = await GetAspNetCoreEndpoint();

        // Login with core app
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();

        // Login
        await Page.Locator("a:has-text(\"Log In\")").ClickAsync();
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$UserName\"]").FillAsync(username);
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$Password\"]").FillAsync("PasswordA");
        await Page.Locator(@"input:has-text(""Login"")").ClickAsync();
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Make sure framework app also logged in
        await Page.GotoAsync(frameworkAppEndpoint);
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Logout").ClickAsync();
        await Expect(Page.Locator(@"text=Login")).ToBeVisibleAsync();
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    private async ValueTask<string> GetAspNetCoreEndpoint()
    {
        var app = await aspire.GetApplicationAsync();
        return app.GetEndpoint("core", "https").AbsoluteUri;
    }

    private async ValueTask<string> GetAspNetFrameworkEndpoint()
    {
        var app = await aspire.GetApplicationAsync();
        return app.GetEndpoint("framework", "https").AbsoluteUri;
    }
}
