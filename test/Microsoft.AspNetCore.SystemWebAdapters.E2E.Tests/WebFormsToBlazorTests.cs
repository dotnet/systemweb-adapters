// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class WebFormsToBlazorTests(AspireFixture<WebFormsToBlazorAppHost> aspire, ITestOutputHelper output) : DebugPageTest, IClassFixture<AspireFixture<WebFormsToBlazorAppHost>>
{
    [WindowsOnlyFact]
    public async Task BlazorComponentInWebForms()
    {
        using var scope = aspire.GetApplicationScope(output);
        var endpoint = scope.App.GetEndpoint("core", "https");

        await Page.GotoAsync(new Uri(endpoint, "/about").ToString());
        await Expect(Page.Locator("text=HelloWorld from ASP.NET Core")).ToBeVisibleAsync(DefaultVisibleTimeout);
        await Expect(Page.Locator("text=Current path: /about")).ToBeVisibleAsync(DefaultVisibleTimeout);
    }
}
