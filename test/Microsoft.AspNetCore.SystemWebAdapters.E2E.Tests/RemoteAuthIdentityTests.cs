// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

public class AuthIdentityTests(AspireFixture<AuthRemoteIdentityAppHost, ContainerAspireFixtureOptions> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<AuthRemoteIdentityAppHost, ContainerAspireFixtureOptions>>
{
    [WindowsWithLinuxContainersTheory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCCoreAppCanLogoutBothApps(string name)
    {
        using var scope = aspire.GetApplicationScope(output);
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = GetEndpoint(scope, name);
        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);

        await Page.GotoAsync(frameworkAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator("text=My ASP.NET Application")).ToBeVisibleAsync(DefaultVisibleTimeout);

        await RegisterUser(email);

        // Make sure core app also logged in
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync(DefaultVisibleTimeout);

        // Logout on core app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync(DefaultVisibleTimeout);
        await Page.GotoAsync(frameworkAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync(DefaultVisibleTimeout);
    }

    [WindowsWithLinuxContainersTheory]
    [InlineData("core")]
    [InlineData("owin")]
    public async Task MVCAppCanLogoutBothApps(string name)
    {
        using var scope = aspire.GetApplicationScope(output);
        var email = $"{Path.GetRandomFileName()}@test.com";
        var coreAppEndpoint = GetEndpoint(scope, name);
        var frameworkAppEndpoint = GetAspNetFrameworkEndpoint(scope);

        // Login with core app
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator("text=ASP.NET Core")).ToBeVisibleAsync(DefaultVisibleTimeout);

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
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync(DefaultVisibleTimeout);

        // Logout on framework app and make sure both logged out
        await Page.Locator(@"text=Log out").ClickAsync();
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync(DefaultVisibleTimeout);
        await Page.GotoAsync(coreAppEndpoint.AbsoluteUri);
        await Expect(Page.Locator(@"text=Log in")).ToBeVisibleAsync(DefaultVisibleTimeout);
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
        await Expect(Page.Locator($"text=Hello {email}!")).ToBeVisibleAsync(DefaultVisibleTimeout);
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
