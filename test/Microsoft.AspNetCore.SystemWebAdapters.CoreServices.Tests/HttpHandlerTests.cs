// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests;

public class HttpHandlerTests
{
    [Theory]
    [InlineData("test.txt", "/test.txt", true)]
    [InlineData("test.txt", "/folder/test.txt", true)]
    [InlineData("/test.txt", "/folder/test.txt", false)]
    [InlineData("/test.txt", "/test.txt", true)]
    [InlineData("test.txt", "/prefix_test.txt", false)]
    [InlineData("*.txt", "/test.txt", true)]
    [InlineData("*.txt", "/folder/test.txt", true)]
    [InlineData("*.txt", "/blah.txt", true)]
    [InlineData("*.txt", "/blah.txt2", false)]
    [InlineData("/path1/path2/file.txt", "/path1/path2/file.txt", true)]
    [InlineData("path1/path2/file.txt", "/path1/path2/file.txt", true)]
    [InlineData("path1/path2/file.txt", "/path0/path1/path2/file.txt", true)]
    public async Task MapHandler(string pattern, string path, bool wasHit)
    {
        // Arrange
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
                           endpoints.MapHttpHandler<EmptyHandler>(pattern);
                       });
                   });
           })
           .StartAsync();

        // Act
        using var result = await host.GetTestClient().GetAsync(new Uri(path, UriKind.Relative));

        // Assert
        Assert.Equal(wasHit, result.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("test.txt", "/test.txt", "/test.txt", "")]
    [InlineData("test.txt", "/test.txt/", "/test.txt", "/")]
    [InlineData("*.txt", "/test.txt/", "/test.txt", "/")]
    [InlineData("*.txt", "/test.txt/sub", "/test.txt", "/sub")]
    [InlineData("*.txt", "/folder/test.txt/sub", "/folder/test.txt", "/sub")]
    [InlineData("/path1/path2/file.txt", "/path1/path2/file.txt", "/path1/path2/file.txt", "")]
    [InlineData("/path1/path2/file.txt", "/path1/path2/file.txt/sub", "/path1/path2/file.txt", "/sub")]
    [InlineData("path1/path2/file.txt", "/path1/path2/file.txt", "/path1/path2/file.txt", "")]
    [InlineData("path1/path2/file.txt", "/path1/path2/file.txt/sub", "/path1/path2/file.txt", "/sub")]
    [InlineData("path1/path2/file.txt", "/path0/path1/path2/file.txt", "/path0/path1/path2/file.txt", "")]
    [InlineData("path1/path2/file.txt", "/path0/path1/path2/file.txt/sub", "/path0/path1/path2/file.txt", "/sub")]
    public async Task FilePath(string pattern, string path, string filePath, string pathInfo)
    {
        // Arrange
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
                          endpoints.MapHttpHandler<MyHttpHandler>(pattern, new() { AllowPathInfo = true });
                      });
                  });
          })
          .StartAsync();

        // Act
        using var stream = await host.GetTestClient().GetStreamAsync(new Uri(path, UriKind.Relative));
        using var reader = new StreamReader(stream);

        // Assert
        Assert.Equal(path, await reader.ReadLineAsync());
        Assert.Equal(filePath, await reader.ReadLineAsync());
        Assert.Equal(pathInfo, await reader.ReadLineAsync());
    }

    [Theory]
    [InlineData("GET", true, "GET")]
    [InlineData("GET", false, "PUT")]
    [InlineData("GET", true, "PUT", "GET")]
    public async Task HandlerVerbs(string methodString, bool hit, params string[] allowedMethods)
    {
        // Arrange
        var method = new HttpMethod(methodString);

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
                            endpoints.MapHttpHandler<EmptyHandler>("/", new MappedHttpHandlerOptions { Verbs = allowedMethods });
                        });
                    });
            })
            .StartAsync();

        using var request = new HttpRequestMessage(method, new Uri("/", UriKind.Relative));

        // Act
        using var result = await host.GetTestClient().SendAsync(request);

        // Assert
        Assert.Equal(hit, result.IsSuccessStatusCode);
    }

    private sealed class EmptyHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
        }
    }

    private sealed class MyHttpHandler : IHttpHandler
    {
        public bool IsReusable => true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA3002:Review code for XSS vulnerabilities", Justification = "Test")]
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Output.WriteLine(context.Request.Path!);
            context.Response.Output.WriteLine(context.Request.FilePath);
            context.Response.Output.WriteLine(context.Request.PathInfo);
        }
    }
}
