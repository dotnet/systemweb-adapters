// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[Collection(nameof(SelfHostedTests))]
public class BuilderTests
{
    [Fact]
    public async Task MultipleServiceRegistrationInvocations()
    {
        // Arrange
        const string ExpectedResult = "Hello world";
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSystemWebAdapters();

                        // Add it again to ensure we're only adding services once
                        services.AddSystemWebAdapters();
                    })
                    .Configure(app =>
                    {
                        app.UseSystemWebAdapters();

                        app.Run(ctx => ctx.Response.WriteAsync(ExpectedResult));
                    });
            })
            .StartAsync();

        // Act
        var result = await host.GetTestClient().GetStringAsync(new Uri("/", UriKind.Relative));

        // Assert
        Assert.Equal(ExpectedResult, result);
    }
}
