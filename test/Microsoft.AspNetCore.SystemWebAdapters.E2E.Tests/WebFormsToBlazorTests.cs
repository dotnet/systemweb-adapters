using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class WebFormsToBlazorTests(AspireFixture<WebFormsToBlazorAppHost> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<WebFormsToBlazorAppHost>>
{
    [WindowsOnlyFact]
    public async Task BlazorComponentInWebForms()
    {
        using var scope = await aspire.GetApplicationScopeAsync(output);
        var endpoint = scope.App.GetEndpoint("core", "https");

        await Page.GotoAsync(new Uri(endpoint, "/about").ToString());
        await Expect(Page.Locator("text=HelloWorld from ASP.NET Core")).ToBeVisibleAsync(DefaultVisibleTimeout);
        await Expect(Page.Locator("text=Current path: /about")).ToBeVisibleAsync(DefaultVisibleTimeout);
    }
}
