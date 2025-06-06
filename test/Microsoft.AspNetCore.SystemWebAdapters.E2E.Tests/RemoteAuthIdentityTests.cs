using Aspire.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Playwright.Xunit;
using Projects;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class RemoteAuthIdentityTests(AspireFixture<AuthRemoteIdentityAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<AuthRemoteIdentityAppHost>>
{
    [Fact]
    public async Task MVCCoreAppCanLogoutBothApps()
    {
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = await GetAspNetCoreEndpoint();
        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();

        await Page.GotoAsync(frameworkAppEndpoint);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();

        await RegisterUser(email);

        // Make sure core app also logged in
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(frameworkAppEndpoint);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MVCAppCanLogoutBothApps()
    {
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = await GetAspNetCoreEndpoint();
        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();

        // Login with core app
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();

        // Create the user
        await RegisterUser(email);

        // Make sure framework app also logged in
        await Page.GotoAsync(frameworkAppEndpoint);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(coreAppEndpoint);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    private async Task RegisterUser(string email)
    {
        const string Password = "1qaz!QAZ";

        // Create the user
        await Page.Locator("text=Register").ClickAsync();
        await Page.Locator("input[name=Email]").FillAsync(email);
        await Page.Locator("input[name=Password]").FillAsync(Password);
        await Page.Locator("input[name=ConfirmPassword]").FillAsync(Password);
        await Page.Locator(@"input:has-text(""Register"")").ClickAsync();
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();
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
