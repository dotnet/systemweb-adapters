using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
public class FormsSampleTest : PageTest
{
    private const string FrameworkAppUrl = "https://localhost:44394/";
    private const string CoreAppUrl = "https://localhost:7080/";

    [Test]
    public async Task CoreAppCanLogout()
    {
        var username = "User1";

        await Page.GotoAsync(FrameworkAppUrl);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();

        // Login
        await Page.Locator("a:has-text(\"Log In\")").ClickAsync();
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$UserName\"]").TypeAsync(username);
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$Password\"]").TypeAsync("PasswordA");
        await Page.Locator(@"input:has-text(""Login"")").ClickAsync();
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Make sure core app also logged in
        await Page.GotoAsync(CoreAppUrl);
        await Expect(Page.Locator($"text=Hello {username}!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator($"text=You have been logged out")).ToBeVisibleAsync();

        // Note: Logout on core app doesn't logout framework app
        //await Page.GotoAsync(FrameworkAppUrl);
        //await Expect(Page.Locator(@"text=Login")).ToBeVisibleAsync();
    }

    [Test]
    public async Task FrameworkCanLogoutBothApps()
    {
        var username = "User1";

        // Login with core app
        await Page.GotoAsync(CoreAppUrl);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();

        // Login
        await Page.Locator("a:has-text(\"Log In\")").ClickAsync();
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$UserName\"]").TypeAsync(username);
        await Page.Locator("input[name=\"ctl00\\$MainContent\\$Password\"]").TypeAsync("PasswordA");
        await Page.Locator(@"input:has-text(""Login"")").ClickAsync();
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Make sure framework app also logged in
        await Page.GotoAsync(FrameworkAppUrl);
        await Expect(Page.Locator($"text=Welcome back, {username}!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Logout").ClickAsync();
        await Expect(Page.Locator(@"text=Login")).ToBeVisibleAsync();
        await Page.GotoAsync(CoreAppUrl);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }
}
