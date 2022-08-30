// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class SetDefaultResponseHeadersMiddlewareTests
{
    [Fact]
    public async Task NoHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new SetDefaultResponseHeadersMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);

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
        context.Response.Headers[name] = value;

        var next = Task (HttpContextCore context) => Task.CompletedTask;
        var middleware = new SetDefaultResponseHeadersMiddleware(new(next));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(value, context.Response.Headers[name]);
    }
}
