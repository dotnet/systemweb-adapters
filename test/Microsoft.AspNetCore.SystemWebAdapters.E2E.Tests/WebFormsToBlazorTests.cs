using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class WebFormsToBlazorTests(AspireFixture<WebFormsToBlazorAppHost> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<WebFormsToBlazorAppHost>>
{
    [WindowsOnlyFact(Skip = "Failing to load")]
    public async Task CanNavigateToWebFormsPage()
    {
        using var scope = await aspire.GetApplicationScopeAsync(output);
        var endpoint = scope.App.GetEndpoint("core");

        await Page.GotoAsync(new Uri(endpoint, "/about").ToString());
        await Page.WaitForSelectorAsync("text=HelloWorld from ASP.NET Core");
        await Page.WaitForSelectorAsync("text=Current path: /about");
    }
}
