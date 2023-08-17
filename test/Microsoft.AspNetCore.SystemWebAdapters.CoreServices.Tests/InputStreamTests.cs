// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public partial class InputStreamTests
{
    private readonly string ContentValue = Guid.NewGuid().ToString();

    [Fact]
    public async Task InputStreamThrowsWithNoMetadata()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(ContentValue, context =>
        {
            _ = context.Request.InputStream.ReadByte();
        }));
    }

    [Fact]
    public async Task EntityModeCheckGetBuffered()
    {
        await RunAsync(context =>
        {
            Assert.Equal(ReadEntityBodyMode.None, context.Request.ReadEntityBodyMode);

            _ = context.Request.GetBufferedInputStream();

            Assert.Equal(ReadEntityBodyMode.Buffered, context.Request.ReadEntityBodyMode);
        });
    }

    [Fact]
    public async Task EntityModeCheckGetBufferless()
    {
        await RunAsync(context =>
        {
            Assert.Equal(ReadEntityBodyMode.None, context.Request.ReadEntityBodyMode);

            _ = context.Request.GetBufferlessInputStream();

            Assert.Equal(ReadEntityBodyMode.Bufferless, context.Request.ReadEntityBodyMode);
        });
    }

    [Fact]
    public async Task EntityModeCheckGetBufferedThenBufferless()
    {
        await RunAsync(context =>
        {
            Assert.Equal(ReadEntityBodyMode.None, context.Request.ReadEntityBodyMode);

            _ = context.Request.GetBufferlessInputStream();

            Assert.Equal(ReadEntityBodyMode.Bufferless, context.Request.ReadEntityBodyMode);

            Assert.Throws<InvalidOperationException>(() => context.Request.GetBufferedInputStream());
        });
    }

    [Fact]
    public async Task InputStreamWithMetadata()
    {
        // Act
        var result = await RunAsync(ContentValue, context =>
        {
            context.Request.InputStream.CopyTo(context.Response.OutputStream);
        }, builder => builder.PreBufferRequestStream());

        // Assert
        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task InputStreamCanSeek()
    {
        // Act
        var result = await RunAsync(ContentValue, context =>
        {
            Assert.Equal(ContentValue.Length, context.Request.InputStream.Length);

            context.Request.InputStream.Position = 1;
            context.Request.InputStream.CopyTo(context.Response.OutputStream);
        }, builder => builder.PreBufferRequestStream());

        // Assert
        Assert.Equal(ContentValue[1..], result);
    }

    [Fact]
    public async Task GetBufferedInputStreamNoMetadata()
    {
        await RunAsync(ContentValue, context =>
        {
            var stream = context.Request.GetBufferedInputStream();

            Assert.Equal(0, stream.Length);
            Assert.Equal((byte)ContentValue[0], stream.ReadByte());
            stream.Position = 0;
            Assert.Equal((byte)ContentValue[0], stream.ReadByte());
        });
    }

    [Fact]
    public async Task GetBufferlessInputStreamNoMetadata()
    {
        // Act
        var result = await RunAsync(ContentValue, context =>
        {
            var stream = context.Request.GetBufferlessInputStream();

            Assert.True(stream.CanRead);
            Assert.False(stream.CanWrite);
            Assert.False(stream.CanSeek);

            stream.CopyTo(context.Response.OutputStream);
        });

        // Assert
        Assert.Equal(ContentValue, result);
    }

    private static Task<string> RunAsync(Action<HttpContext> action, Action<RouteHandlerBuilder>? route = null)
        => RunAsync(string.Empty, action, route);

    private static async Task<string> RunAsync(string input, Action<HttpContext> action, Action<RouteHandlerBuilder>? route = null)
    {
        route ??= _ => { };

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
                            route(endpoints.Map("/", (HttpContextCore context) => action(context)));
                        });
                    });
            })
            .StartAsync();

        try
        {
            using var content = new StringContent(input);
            using var response = await host.GetTestClient().PutAsync(new Uri("/", UriKind.Relative), content);

            return await response.Content.ReadAsStringAsync();
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
