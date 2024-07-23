// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
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
    public async Task OutputPipeIsFlushed()
    {
        const string Message = "hello world from pipe!";

        var result = await RunAsync(context =>
        {
            context.AsAspNetCore().Response.BodyWriter.Write(Encoding.UTF8.GetBytes(Message));
        });

        Assert.Equal(Message, result);
    }

    [Fact]
    public async Task OutputPipeIsMarkedCompleteIfRequestIsComplete()
    {
        var result = await RunAsync(async context =>
        {
            await context.AsAspNetCore().Response.CompleteAsync();

            Assert.Throws<InvalidOperationException>(() => context.AsAspNetCore().Response.BodyWriter.Write("Hello world"u8));
        });

        Assert.Empty(result);
    }

    [Fact]
    public async Task BufferedOutputIsFlushedOnceWithStart()
    {
        var result = await RunAsync(async context =>
        {
            context.Response.Write(ContentValue);
            await context.Response.AsAspNetCore().StartAsync();
        }, builder => builder.BufferResponseStream());

        Assert.Equal(ContentValue, result);
    }

    [Fact]
    public async Task BufferedOutputIsFlushedOnceWithComplete()
    {
        var result = await RunAsync(async context =>
        {
            context.Response.Write(ContentValue);
            await context.Response.AsAspNetCore().CompleteAsync();
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
    public async Task FilterInstalled()
    {
        // Arrange
        const string Message = "Hello world!";
        var bytes = Encoding.UTF8.GetBytes(Message);

        TrackingStream filter = default!;

        // Act
        var result = await RunAsync(context =>
        {
            context.Response.Filter = filter = new TrackingStream(context.Response.Filter);
            context.Response.OutputStream.Write(bytes);
        }, builder => builder.BufferResponseStream());

        // Assert
        Assert.NotNull(filter);
        Assert.Equal(bytes, filter.Bytes);
        Assert.Equal(Message, result);
        Assert.True(filter.IsDisposed);
    }

    [Fact]
    public async Task FilterUninstalled()
    {
        // Arrange
        const string Message = "Hello world!";
        var bytes = Encoding.UTF8.GetBytes(Message);

        TrackingStream filter = default!;

        // Act
        var result = await RunAsync(context =>
        {
            context.Response.Filter = filter = new TrackingStream(context.Response.Filter);
            context.Response.OutputStream.Write(bytes);
            context.Response.Filter = null;
        }, builder => builder.BufferResponseStream());

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter.Bytes);
        Assert.Equal(Message, result);
        Assert.False(filter.IsDisposed);
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

    [Fact]
    public async Task BufferMultipleTimes()
    {
        const string Result = "text";

        // Act
        var result = await RunAsync(context =>
        {
            var feature = context.AsAspNetCore().Features.GetRequiredFeature<IHttpResponseBufferingFeature>();

            feature.EnableBuffering(1024, default);
            feature.EnableBuffering(1024, default);

            context.Response.Write(Result);
        });

        Assert.Equal(Result, result);
    }

    [Fact]
    public async Task BufferOutputIsNotEnabled()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write(context.Response.BufferOutput.ToString());
        });

        Assert.Equal("False", result);
    }

    [Fact]
    public async Task BufferedOutputIsEnabled()
    {
        var result = await RunAsync(context =>
        {
            context.Response.Write(context.Response.BufferOutput.ToString());
        }, builder => builder.BufferResponseStream());

        Assert.Equal("True", result);
    }

    [Fact]
    public async Task BufferingCanBeDisabled()
    {
        await RunAsync(middleware: (ctx, next) =>
        {
            ctx.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(1024, default);
            Assert.True(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);
            ctx.Features.GetRequired<IHttpResponseBodyFeature>().DisableBuffering();
            Assert.False(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            return next(ctx);
        });
    }

    [Fact]
    public async Task BufferingCanBeDisabledWithSuppressContent()
    {
        var result = await RunAsync(middleware: async (ctx, next) =>
        {
            ctx.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(1024, default);
            Assert.True(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.WriteAsync("before ");

            ctx.Features.GetRequired<IHttpResponseContentFeature>().SuppressContent = true;

            ctx.Features.GetRequired<IHttpResponseBodyFeature>().DisableBuffering();
            Assert.False(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.WriteAsync("after");

            await next(ctx);
        });

        Assert.Equal("after", result);
    }

    [Fact]
    public async Task BufferingCanBeDisabledAndFlushes()
    {
        var result = await RunAsync(middleware: async (ctx, next) =>
        {
            ctx.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(1024, default);
            Assert.True(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.WriteAsync("before ");

            ctx.Features.GetRequired<IHttpResponseBodyFeature>().DisableBuffering();
            Assert.False(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.WriteAsync("after");

            await next(ctx);
        });

        Assert.Equal("before after", result);
    }

    [Fact]
    public async Task BufferingCannotBeEnabledIfWritingHasBegun()
    {
        await RunAsync(middleware: async (ctx, next) =>
        {
            await ctx.Response.WriteAsync("start");

            Assert.Throws<InvalidOperationException>(() =>
            {
                ctx.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(1024, default);
            });

            await next(ctx);
        });
    }

    [Fact]
    public async Task BufferingCanBeDisabledAndFlushesUsingPipe()
    {
        var result = await RunAsync(middleware: async (ctx, next) =>
        {
            ctx.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(1024, default);
            Assert.True(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("before "));

            ctx.Features.GetRequired<IHttpResponseBodyFeature>().DisableBuffering();
            Assert.False(ctx.Features.GetRequired<IHttpResponseBufferingFeature>().IsEnabled);

            await ctx.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("after"));

            await next(ctx);
        });

        Assert.Equal("before after", result);
    }

    private static Task<string> RunAsync(Action<HttpContext> action, Action<IEndpointConventionBuilder>? builder = null, Func<Http.HttpContext, RequestDelegate, Task>? middleware = null)
        => RunAsync(ctx =>
        {
            action(ctx);
            return Task.CompletedTask;
        }, builder, middleware);

    private static async Task<string> RunAsync(Func<HttpContext, Task>? endpointAction = null, Action<IEndpointConventionBuilder>? builder = null, Func<Http.HttpContext, RequestDelegate, Task>? middleware = null)
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

                        if (middleware is { })
                        {
                            app.Use(middleware);
                        }

                        app.UseEndpoints(endpoints =>
                        {
                            builder(endpoints.Map("/", (HttpContextCore context) =>
                            {
                                if (endpointAction is { })
                                {
                                    endpointAction(context);
                                }
                            }));
                        });
                    });
            })
            .StartAsync();

        var uri = new Uri("/", UriKind.Relative);

        try
        {
            return await host.GetTestClient().GetStringAsync(uri).ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private sealed class TrackingStream : Stream
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is not owned by this instance")]
        private readonly Stream _stream;
        private readonly List<byte> _list = new();

        public TrackingStream(Stream other)
        {
            _stream = other;
        }

        public byte[] Bytes => _list.ToArray();

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _list.AddRange(buffer.AsMemory(offset, count).ToArray());
            _stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }
    }
}
