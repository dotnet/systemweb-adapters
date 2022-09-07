using Microsoft.Playwright.NUnit;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
public class MvcSampleTest : PageTest
{
    [Test]
    public async Task MVCCoreAppCanLogoutBothApps()
    {
        // Login with mvc app
        await Page.GotoAsync("https://localhost:44339/");
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();
        await Page.Locator("text=Log in").ClickAsync();
        await Expect(Page.Locator("text=Use a local account to log in.")).ToBeVisibleAsync();
        await Page.Locator("input[name=Email]").TypeAsync("test@test.com");
        await Page.Locator("input[name=Password]").TypeAsync("1qaz!QAZ");
        await Page.Locator(@"input:has-text(""Log in"")").ClickAsync();
        await Expect(Page.Locator(@"text=Hello test@test.com!")).ToBeVisibleAsync();

        // Make sure core app also logged in
        await Page.GotoAsync("https://localhost:55442/");
        await Expect(Page.Locator(@"text=Hello test@test.com!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync("https://localhost:44339/");
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MVCAppCanLogoutBothApps()
    {
        // Login with core app
        await Page.GotoAsync("https://localhost:55442/");
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();
        await Page.Locator("text=Log in").ClickAsync();
        await Expect(Page.Locator("text=Use a local account to log in.")).ToBeVisibleAsync();
        await Page.Locator("input[name=Email]").TypeAsync("test@test.com");
        await Page.Locator("input[name=Password]").TypeAsync("1qaz!QAZ");
        await Page.Locator(@"input:has-text(""Log in"")").ClickAsync();
        await Expect(Page.Locator(@"text=Hello test@test.com!")).ToBeVisibleAsync();

        // Make sure framework app also logged in
        await Page.GotoAsync("https://localhost:44339/");
        await Expect(Page.Locator(@"text=Hello test@test.com!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync("https://localhost:55442/");
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }
}
