// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class CachePolicyMiddlewareTests
{
    [Fact]
    public async Task NoHeadersWithContent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);
        context.Features.Set<ITimestampFeature>(startupFeature);
        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new CachePolicyMiddleware(new(next));
        var data = new MemoryStream(Encoding.UTF8.GetBytes("ArbitraryContent"));
        context.Response.Body = data;
        context.Response.ContentLength = data.Length;

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Equal(3, context.Response.Headers.Count);
        Assert.Equal("text/html", context.Response.Headers.ContentType);
        Assert.Equal("private", context.Response.Headers.CacheControl);
        Assert.Equal(data.Length, context.Response.ContentLength);
    }

    [Fact]
    public async Task NoHeadersWithoutContent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);
        context.Features.Set<ITimestampFeature>(startupFeature);
        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new CachePolicyMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Single(context.Response.Headers);
        Assert.False(context.Response.Headers.ContainsKey("Content-Type"));
    }

    [Theory]
    [InlineData("Content-Type", "some-content-type")]
    public async Task Existing(string name, string value)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);
        context.Features.Set<ITimestampFeature>(startupFeature);

        context.Response.Headers[name] = value;

        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new CachePolicyMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Equal(value, context.Response.Headers[name]);
    }

    [Theory]
    [InlineData("Content-Type", "some-content-type")]
    public async Task AddedInLaterMiddleware(string name, string value)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);
        context.Features.Set<ITimestampFeature>(startupFeature);

        var next = Task (HttpContextCore context) =>
        {
            var adapter = context.AsSystemWeb();
            adapter.Response.AppendHeader(name, value);

            return Task.CompletedTask;
        };
        var middleware = new CachePolicyMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Equal(value, context.Response.Headers[name]);
    }

    // The default feature does not include a functional `OnStarting` method
    private sealed class StartupCallbackFeature : HttpResponseFeature, ITimestampFeature
    {
        private Func<Task>? _callback;

        public static DateTimeOffset DefaultTimestamp { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset Timestamp => DefaultTimestamp;

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            _callback = () => callback(state);
        }

        public Task RunAsync()
        {
            if (_callback is null)
            {
                throw new InvalidOperationException();
            }

            return _callback();
        }
    }
}
