using System.Text.Json;
using System.Text.Json.Serialization;
using Aspire.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Playwright.Xunit;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class AuthIdentityTests(AspireFixture<AuthRemoteIdentityAppHost> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<AuthRemoteIdentityAppHost>>
{
    [WindowsOnlyTheory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCCoreAppCanLogoutBothApps(string name)
    {
        using var scope = await aspire.GetApplicationScopeAsync(output);
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = GetEndpoint(scope, name);
        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);

        await Page.GotoAsync(frameworkAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync();

        await RegisterUser(email);

        // Make sure core app also logged in
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(frameworkAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    [WindowsOnlyTheory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCAppCanLogoutBothApps(string name)
    {
        using var scope = await aspire.GetApplicationScopeAsync(output);
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = GetEndpoint(scope, name);
        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);

        // Login with core app
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync();

        // Create the user
        await RegisterUser(email);

        // Check the /user endpoint for ClaimsIdentity.Current setting
        var user = await Page.GotoAsync(new Uri(coreAppEndpoint, "/user").AbsoluteUri);
        Assert.NotNull(user);
        var userResult = await user.JsonAsync<UserResult>();
        Assert.Equal(email, userResult.Name);
        Assert.True(userResult.IsAuthenticated);

        // Make sure framework app also logged in
        await Page.GotoAsync(frameworkAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync();
    }

    private async Task RegisterUser(string email)
    {
        var password = new GenerateParameterDefault().GetDefaultValue();

        // Create the user
        await Page.Locator("text=Register").ClickAsync();
        await Page.Locator("input[name=Email]").FillAsync(email);
        await Page.Locator("input[name=Password]").FillAsync(password);
        await Page.Locator("input[name=ConfirmPassword]").FillAsync(password);
        await Page.Locator(@"input:has-text(""Register"")").ClickAsync();
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();
    }

    private static Uri GetEndpoint(IDistributeApplicationScope scope, string name)
    {
        return scope.App.GetEndpoint(name, "https");
    }

    private static Uri GetAspNetFrameworkEndpoint(IDistributeApplicationScope scope) => GetEndpoint(scope, "framework");

    private sealed class UserResult
    {
        [JsonPropertyName("isAuthenticated")]
        public bool IsAuthenticated { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
