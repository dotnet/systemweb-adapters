// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[Collection(nameof(SelfHostedTests))]
public class HttpContextIntegrationTests
{
    [Fact]
    public Task RequestInfo()
        => RunTest("/", context =>
        {
            Assert.Equal("/", context.Request.Path);
            Assert.Empty(context.Request.Query);

            var adapter = context.Request.GetSystemWebRequest();

            Assert.Equal("", adapter.PathInfo);
            Assert.Equal("/", adapter.FilePath);
            Assert.Equal("/", adapter.Path);
            Assert.Empty(adapter.QueryString);
        });

    [Fact]
    public Task RewriteRequestPath()
        => RunTest("/", context =>
        {
            var adapter = context.GetSystemWebHttpContext();

            adapter.RewritePath("/some/path?q=1");

            Assert.Equal("/some/path", context.Request.Path);
            Assert.Collection(context.Request.Query,
                q =>
                {
                    Assert.Equal("q", q.Key);
                    Assert.Equal("1", q.Value);
                });

            Assert.Equal("", adapter.Request.PathInfo);
            Assert.Equal("/some/path", adapter.Request.FilePath);
            Assert.Equal("/some/path", adapter.Request.Path);
            Assert.Single(adapter.Request.QueryString);
            Assert.Equal("1", adapter.Request.QueryString["q"]);
        });

    [Fact]
    public Task RewriteRequestPathInfo()
        => RunTest("/", context =>
        {
            var adapter = context.GetSystemWebHttpContext();

            adapter.RewritePath("/some/path", "/pathInfo", "q=1");

            Assert.Equal("/some/path/pathInfo", context.Request.Path);
            Assert.Collection(context.Request.Query,
                q =>
                {
                    Assert.Equal("q", q.Key);
                    Assert.Equal("1", q.Value);
                });

            Assert.Equal("/pathInfo", adapter.Request.PathInfo);
            Assert.Equal("/some/path", adapter.Request.FilePath);
            Assert.Equal("/some/path/pathInfo", adapter.Request.Path);
            Assert.Single(adapter.Request.QueryString);
            Assert.Equal("1", adapter.Request.QueryString["q"]);
        });

    [Fact]
    public Task RewritePathViaCoreApis()
        => RunTest("/", context =>
        {
            var adapter = context.GetSystemWebHttpContext();

            // This is the same as RewriteRequestPathInfo as above to get a custom PathInfo
            adapter.RewritePath("/some/path", "/pathInfo", "q=1");
            context.Request.Path = "/other";

            Assert.Equal("/other", context.Request.Path);
            Assert.Collection(context.Request.Query,
                q =>
                {
                    Assert.Equal("q", q.Key);
                    Assert.Equal("1", q.Value);
                });

            Assert.Equal(string.Empty, adapter.Request.PathInfo);
            Assert.Equal("/other", adapter.Request.FilePath);
            Assert.Equal("/other", adapter.Request.Path);
            Assert.Single(adapter.Request.QueryString);
            Assert.Equal("1", adapter.Request.QueryString["q"]);
        });

    private static async Task RunTest(string path, Action<HttpContextCore> run)
    {
        // Arrange
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddSystemWebAdapters();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseSystemWebAdapters();

                        app.Run(ctx =>
                        {
                            run(ctx);
                            return Task.CompletedTask;
                        });

                    });
            })
            .StartAsync();

        try
        {
            _ = await host.GetTestClient().GetAsync(new Uri(path, UriKind.Relative));
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
