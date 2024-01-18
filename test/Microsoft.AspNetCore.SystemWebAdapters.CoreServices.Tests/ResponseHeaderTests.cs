// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Xunit;

using SameSiteMode = System.Web.SameSiteMode;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class ResponseHeaderTests
{
    private readonly string ContentValue = Guid.NewGuid().ToString();

    [Fact]
    public async Task SetCookie()
    {
        using var result = await RunAsync(context =>
        {
            var cookie = new HttpCookie("test", ContentValue);
            context.Response.Cookies.Add(cookie);
        });

        var cookies = result.Headers.GetValues(HeaderNames.SetCookie).ToList();
        Assert.Single(cookies);
        Assert.Equal($"test={ContentValue}; path=/; samesite=lax", cookies.First());
    }

    [Fact]
    public async Task SetMultipleCookie()
    {
        // Arrange
        var cookie1 = new HttpCookie("test1", Guid.NewGuid().ToString());
        var cookie2 = new HttpCookie("test2", Guid.NewGuid().ToString());

        // Act
        using var result = await RunAsync(context =>
        {
            context.Response.Cookies.Add(cookie1);
            context.Response.Cookies.Add(cookie2);
        });

        // Assert
        var cookies = result.Headers.GetValues(HeaderNames.SetCookie).ToList();
        Assert.Equal(2, cookies.Count);
        Assert.Equal($"test1={cookie1.Value}; path=/; samesite=lax", cookies.First());
        Assert.Equal($"test2={cookie2.Value}; path=/; samesite=lax", cookies.Last());
    }

    [Fact]
    public async Task SetCookieWithPath()
    {
        using var result = await RunAsync(context =>
        {
            var cookie = new HttpCookie("test", ContentValue) { Path = "/abc" };
            context.Response.Cookies.Add(cookie);
        });

        Assert.Equal($"test={ContentValue}; path=/abc; samesite=lax", result.Headers.GetValues(HeaderNames.SetCookie).First());
    }

    [Fact]
    public async Task SetCookieSameSite()
    {
        using var result = await RunAsync(context =>
        {
            var cookie = new HttpCookie("test", ContentValue) { SameSite = SameSiteMode.None };
            context.Response.Cookies.Add(cookie);
        });

        Assert.Equal($"test={ContentValue}; path=/; samesite=none", result.Headers.GetValues(HeaderNames.SetCookie).First());
    }

    [Fact]
    public async Task SetCookieUrlencodedValue()
    {
        // Arrange
        const string cookieValue = "hello|world";
        var cookie = new HttpCookie("test", cookieValue) { SameSite = SameSiteMode.None };

        // Act
        using var result = await RunAsync(context =>
        {
            context.Response.Cookies.Add(cookie);
        });

        // Assert
        Assert.Equal($"test={cookieValue}; path=/; samesite=none", result.Headers.GetValues(HeaderNames.SetCookie).First());
    }

    private static Task<HttpResponseMessage> RunAsync(Action<HttpContext> action, Action<IEndpointConventionBuilder>? builder = null)
        => RunAsync(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        }, builder);

    private static async Task<HttpResponseMessage> RunAsync(Func<HttpContext, Task> action, Action<IEndpointConventionBuilder>? builder = null)
    {
        builder ??= _ => { };

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer(options =>
                    {
                        options.AllowSynchronousIO = true;
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddSystemWebAdapters();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseSystemWebAdapters();
                        app.UseEndpoints(endpoints =>
                        {
                            builder(endpoints.Map("/", (context) => action(context)));
                        });
                    });
            })
            .StartAsync();

        var uri = new Uri("/", UriKind.Relative);

        try
        {
            return await host.GetTestClient().GetAsync(uri).ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
