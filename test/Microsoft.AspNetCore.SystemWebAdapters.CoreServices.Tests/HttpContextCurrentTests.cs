// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class HttpContextCurrentTests
{
    [Fact]
    public void CurrentReturnsNullByDefault()
    {
        Assert.Null(HttpContext.Current);
    }

    [Fact]
    public void SavesByDefaultToHttpContextAccessor()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var context = new DefaultHttpContext();

        // Act
        HttpContext.Current = context;

        // Assert
        Assert.Same(accessor.HttpContext, context);
    }

    [Fact]
    public async Task UsesIHttpContextAccessor()
    {
        // Arrange
        var defaultAccessor = new HttpContextAccessor();
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupAllProperties();

        using var host = await new HostBuilder()
           .ConfigureWebHost(webBuilder =>
           {
               webBuilder
                   .UseTestServer()
                   .ConfigureServices(services =>
                   {
                       services.AddSingleton(accessor.Object);
                       services.AddSystemWebAdapters();
                   })
                   .Configure(app =>
                   {
                   });
           })
           .StartAsync();
        var context = new DefaultHttpContext();

        // Act
        HttpContext.Current = context;

        // Assert
        Assert.Same(context, accessor.Object.HttpContext);
        Assert.Null(defaultAccessor.HttpContext);
    }
}
