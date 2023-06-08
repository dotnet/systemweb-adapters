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

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

public class ResponseStreamTests
{
    private readonly string ContentValue = Guid.NewGuid().ToString();

    [Fact]
    public async Task WriteStreamContent()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write(ContentValue);
        });

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task BufferedOutputIsFlushed()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write(ContentValue);
        }, builder => builder.BufferResponseStream());

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task BufferedOutputIsFlushedOnceWithStart()
    {
        var result = await RunAsync(async context =>
        {
            context.Response.Write(ContentValue);
            await context.Response.UnwrapAdapter().StartAsync();
        }, builder => builder.BufferResponseStream());

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task BufferedOutputIsFlushedOnceWithComplete()
    {
        var result = await RunAsync(async context =>
        {
            context.Response.Write(ContentValue);
            await context.Response.UnwrapAdapter().CompleteAsync();
        }, builder => builder.BufferResponseStream());

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task EndRequest()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write("test");
            context.Response.End();
        }, builder => builder.BufferResponseStream());

        Assert.Equal("test", result);
    }

    [Fact]
    public async Task SuppressContent()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write("test");
            context.Response.SuppressContent = true;
        }, builder => builder.BufferResponseStream());

        Assert.Empty(result);
    }

    [Fact]
    public async Task SuppressContentFalse()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write(ContentValue);
            context.Response.SuppressContent = true;
            context.Response.SuppressContent = false;
        }, builder => builder.BufferResponseStream());

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task ClearContentNoBuffering()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(context =>
        {
            context.Response.ClearContent();
        }));
    }

    [Fact]
    public async Task SetTrueSuppressNoBuffering()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(context =>
        {
            context.Response.SuppressContent = true;
        }));
    }

    [Fact]
    public async Task GetSetFalseSuppressNoBuffering()
    {
        var result = await RunAsync(context =>
        {
            Assert.False(context.Response.SuppressContent);
            context.Response.SuppressContent = false;

            context.Response.Output.Write(ContentValue);
        });

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task ClearContent()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write("part1");
            context.Response.ClearContent();
            context.Response.Write("part2");
        }, builder => builder.BufferResponseStream());

        Assert.Equal("part2", result);
    }

    [Fact]
    public async Task MultipleClearContent()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write("part1");
            context.Response.ClearContent();
            context.Response.Write("part2");
            context.Response.ClearContent();
            context.Response.Write("part3");
            context.Response.ClearContent();
            context.Response.Write("part4");
        }, builder => builder.BufferResponseStream());

        Assert.Equal("part4", result);
    }

    private static Task<string> RunAsync(Action<HttpContext> action, Action<IEndpointConventionBuilder>? builder = null)
        => RunAsync(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        }, builder);

    private static async Task<string> RunAsync(Func<HttpContext, Task> action, Action<IEndpointConventionBuilder>? builder = null)
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
                            builder(endpoints.Map("/", (HttpContextCore context) => action(context)));
                        });
                    });
            })
            .StartAsync();

        var uri = new Uri("/", UriKind.Relative);
        return await host.GetTestClient().GetStringAsync(uri).ConfigureAwait(false);
    }
}
