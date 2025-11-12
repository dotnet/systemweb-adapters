using System.Security.Cryptography;
using Aspire.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Playwright.Xunit;
using Projects;
using System.Text.Json;
using Xunit;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class AuthIdentityTests(AspireFixture<AuthRemoteIdentityAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<AuthRemoteIdentityAppHost>>
{
    [Theory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCCoreAppCanLogoutBothApps(string name)
    {
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = await GetEndpoint(name);
        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();

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

    [Theory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCAppCanLogoutBothApps(string name)
    {
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = await GetEndpoint(name);
        var frameworkAppEndpoint = await GetAspNetFrameworkEndpoint();

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
        var password = CreatePassword();

        // Create the user
        await Page.Locator("text=Register").ClickAsync();
        await Page.Locator("input[name=Email]").FillAsync(email);
        await Page.Locator("input[name=Password]").FillAsync(password);
        await Page.Locator("input[name=ConfirmPassword]").FillAsync(password);
        await Page.Locator(@"input:has-text(""Register"")").ClickAsync();
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync();

        // Passwords must have at least 8 characters with lower, upper, numbers, and symbols
        static string CreatePassword()
        {
            var lower = "abcdefghijklmnopqrstuvwxyz";
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var numbers = "0123456789";
            var symbols = "!@#$%^&*()";

            Span<char> str = stackalloc char[8];

            RandomNumberGenerator.GetItems(lower, str.Slice(0, 2));
            RandomNumberGenerator.GetItems(upper, str.Slice(2, 2));
            RandomNumberGenerator.GetItems(numbers, str.Slice(4, 2));
            RandomNumberGenerator.GetItems(symbols, str.Slice(6, 2));

            RandomNumberGenerator.Shuffle(str);

            return new string(str);
        }
    }

    private async ValueTask<Uri> GetEndpoint(string name)
    {
        var app = await aspire.GetApplicationAsync();
        return app.GetEndpoint(name, "https");
    }

    private ValueTask<Uri> GetAspNetFrameworkEndpoint() => GetEndpoint("framework");

    private sealed class UserResult
    {
        [JsonPropertyName("isAuthenticated")]
        public bool IsAuthenticated { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
