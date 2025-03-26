// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

[Collection(nameof(SelfHostedTests))]
public class CachePolicyMiddlewareTests
{
    [Fact]
    public async Task NoPolicyAdjustments()
    {
        // Arrange/Act
        using var response = await RunAsync(() => new object());

        // Assert
        Assert.Collection(response.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.CacheControl, h.Key);
                Assert.Equal(["private"], h.Value);
            });

        Assert.Collection(response.Content.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.ContentType, h.Key);
                Assert.Equal(["application/json; charset=utf-8"], h.Value);
            });
    }

    [Fact]
    public async Task SetSomePolicy()
    {
        DateTime expires = default;

        // Arrange/Act
        using var response = await RunAsync((HttpContextCore ctx, TimeProvider time) =>
        {
            var response = ctx.AsSystemWeb().Response.Cache;
            var t = ctx.AsSystemWeb().Timestamp;
            expires = ctx.AsSystemWeb().Timestamp.AddDays(2);

            response.SetVaryByCustom(HeaderNames.AcceptEncoding);
            response.SetExpires(expires);
            response.SetETag("ETag1");
            response.SetCacheability(HttpCacheability.Public);
        });

        // Assert
        Assert.Collection(response.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.CacheControl, h.Key);
                Assert.Equal(["public"], h.Value);
            },
            h =>
            {
                Assert.Equal(HeaderNames.ETag, h.Key);
                Assert.Equal(["ETag1"], h.Value);
            },
            h =>
            {
                Assert.Equal(HeaderNames.Vary, h.Key);
                Assert.Equal(["*"], h.Value);
            });

        Assert.Collection(response.Content.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.Expires, h.Key);
                Assert.Equal([Format(expires)], h.Value);
            });
    }

    [Fact]
    public async Task SetSomePolicyThenSetCacheability()
    {
        using var response = await RunAsync((HttpContextCore ctx, TimeProvider time) =>
        {
            // Arrange
            var response = ctx.AsSystemWeb().Response.Cache;

            response.SetVaryByCustom(HeaderNames.AcceptEncoding);
            response.SetExpires(ctx.AsSystemWeb().Timestamp.AddDays(2));
            response.SetETag("ETag1");

            // Act
            response.SetCacheability(HttpCacheability.NoCache);
        });

        // Assert
        Assert.Collection(response.Headers.OrderBy(r => r.Key),
            h =>
            {
                Assert.Equal(HeaderNames.CacheControl, h.Key);
                Assert.Equal(["no-cache"], h.Value);
            },
            h =>
            {
                Assert.Equal(HeaderNames.Pragma, h.Key);
                Assert.Equal(["no-cache"], h.Value);
            });

        Assert.Collection(response.Content.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.Expires, h.Key);
                Assert.Equal(["-1"], h.Value);
            });
    }

    [Fact]
    public async Task SetSomePolicyThenSetNoStore()
    {
        DateTime expires = default;

        using var response = await RunAsync((HttpContextCore ctx, TimeProvider time) =>
        {
            // Arrange
            var response = ctx.AsSystemWeb().Response.Cache;
            expires = ctx.AsSystemWeb().Timestamp.AddDays(2);

            response.SetVaryByCustom(HeaderNames.AcceptEncoding);
            response.SetExpires(expires);
            response.SetETag("ETag1");

            // Act
            response.SetNoStore();
        });

        // Assert
        Assert.Collection(response.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.CacheControl, h.Key);
                Assert.Equal(["no-store, private"], h.Value);
            });

        Assert.Collection(response.Content.Headers,
            h =>
            {
                Assert.Equal(HeaderNames.Expires, h.Key);
                Assert.Equal([Format(expires)], h.Value);
            });
    }

    // NOTE: HttpCachePolicy converts times to UTC for formatting
    private static string Format(DateTime dt) => dt.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo);

    private static async Task<HttpResponseMessage> RunAsync(Delegate run)
    {
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
                            endpoints.MapGet("/", run)
                                .BufferResponseStream();
                        });
                    });
            })
            .StartAsync();

        try
        {
            return await host.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
