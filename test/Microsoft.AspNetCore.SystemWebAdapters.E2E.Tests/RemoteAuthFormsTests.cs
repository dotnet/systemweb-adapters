using Aspire.Hosting;
using Microsoft.Playwright.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class RemoteAuthFormsTests(AspireFixture<Projects.AuthRemoteFormsAuthAppHost> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<Projects.AuthRemoteFormsAuthAppHost>>
{
    [WindowsOnlyFact]
    public async Task CoreAppCanLogout()
    {
        var username = "User1";

        using var scope = await aspire.GetApplicationScopeAsync(output);
        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);
        var coreAppEndpoint = GetAspNetCoreEndpoint(scope);

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

    [WindowsOnlyFact]
    public async Task FrameworkCanLogoutBothApps()
    {
        var username = "User1";

        using var scope = await aspire.GetApplicationScopeAsync(output);

        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);
        var coreAppEndpoint = GetAspNetCoreEndpoint(scope);

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

    private static string GetAspNetCoreEndpoint(IDistributeApplicationScope scope)
    {
        return scope.App.GetEndpoint("core", "https").AbsoluteUri;
    }

    private static string GetAspNetFrameworkEndpoint(IDistributeApplicationScope scope)
    {
        return scope.App.GetEndpoint("framework", "https").AbsoluteUri;
    }
}
