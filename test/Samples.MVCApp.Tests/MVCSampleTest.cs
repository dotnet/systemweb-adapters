using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
public class MvcSampleTest : PageTest
{
    private const string MvcAppUrl = "https://localhost:44339/";
    private const string MvcCoreAppUrl = "https://localhost:55442/";

    [Test]
    public async Task MVCCoreAppCanLogoutBothApps()
    {
        var email = $"{Path.GetRandomFileName()}@test.com";

        await Page.GotoAsync(MvcAppUrl);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();

        // Create the user
        await Page.Locator("text=Register").ClickAsync();
        await Page.Locator("input[name=Email]").TypeAsync(email);
        await Page.Locator("input[name=Password]").TypeAsync("1qaz!QAZ");
        await Page.Locator("input[name=ConfirmPassword]").TypeAsync("1qaz!QAZ");
        await Page.Locator(@"input:has-text(""Register"")").ClickAsync();
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Make sure core app also logged in
        await Page.GotoAsync(MvcCoreAppUrl);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(MvcAppUrl);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    [Test]
    public async Task MVCAppCanLogoutBothApps()
    {
        var email = $"{Path.GetRandomFileName()}@test.com";

        // Login with core app
        await Page.GotoAsync(MvcCoreAppUrl);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();

        // Create the user
        await Page.Locator("text=Register").ClickAsync();
        await Page.Locator("input[name=Email]").TypeAsync(email);
        await Page.Locator("input[name=Password]").TypeAsync("1qaz!QAZ");
        await Page.Locator("input[name=ConfirmPassword]").TypeAsync("1qaz!QAZ");
        await Page.Locator(@"input:has-text(""Register"")").ClickAsync();
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Make sure framework app also logged in
        await Page.GotoAsync(MvcAppUrl);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(MvcCoreAppUrl);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }
}
