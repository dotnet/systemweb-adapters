using Projects;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class WebFormsToBlazorTests(AspireFixture<WebFormsToBlazorAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<WebFormsToBlazorAppHost>>
{
    [Fact(Skip = "Failing to load")]
    public async Task CanNavigateToWebFormsPage()
    {
        var app = await aspire.GetApplicationAsync();
        var endpoint = app.GetEndpoint("core");

        await Page.GotoAsync(new Uri(endpoint, "/about").ToString());
        await Page.WaitForSelectorAsync("text=HelloWorld from ASP.NET Core");
        await Page.WaitForSelectorAsync("text=Current path: /about");
    }
}
