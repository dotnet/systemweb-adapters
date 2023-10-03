// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class SetDefaultResponseHeadersMiddlewareTests
{
    [Fact]
    public async Task NoHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);
        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new SetDefaultResponseHeadersMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Equal(2, context.Response.Headers.Count);
        Assert.Equal("text/html", context.Response.Headers.ContentType);
        Assert.Equal("private", context.Response.Headers.CacheControl);
    }

    [Theory]
    [InlineData("Content-Type", "some-content-type")]
    [InlineData("Cache-Control", "cache-value")]
    public async Task Existing(string name, string value)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var startupFeature = new StartupCallbackFeature();
        context.Features.Set<IHttpResponseFeature>(startupFeature);

        context.Response.Headers[name] = value;

        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new SetDefaultResponseHeadersMiddleware(new(next));

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

        var next = Task (HttpContextCore context) =>
        {
            var adapter = context.GetSystemWebHttpContext();
            adapter.Response.AppendHeader(name, value);

            return Task.CompletedTask;
        };
        var middleware = new SetDefaultResponseHeadersMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);
        await startupFeature.RunAsync();

        // Assert
        Assert.Equal(value, context.Response.Headers[name]);
    }

    // The default feature does not include a functional `OnStarting` method
    private sealed class StartupCallbackFeature : HttpResponseFeature
    {
        private Func<Task>? _callback;

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
