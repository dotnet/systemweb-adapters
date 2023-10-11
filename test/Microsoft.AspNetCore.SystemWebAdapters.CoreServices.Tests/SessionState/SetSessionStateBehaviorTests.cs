// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests.SessionState;

[Collection(nameof(SelfHostedTests))]
public class SetSessionStateBehaviorTests
{
    [InlineData("/disabled", "Session:null")]
    [InlineData("/readonly", "ReadOnly:True")]
    [InlineData("/required", "ReadOnly:False")]
    [Theory]
    public async Task TestSetSessionStateBehavior(string endpoint, string expected)
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
                      services.AddSystemWebAdapters()
                        .WrapAspNetCoreSession();
                      services.AddDistributedMemoryCache();
                  })
                  .Configure(app =>
                  {
                      app.UseRouting();
                      app.Use((ctx, next) =>
                      {
                          SetSessionState(ctx);
                          return next(ctx);
                      });
                      app.UseSession();
                      app.UseSystemWebAdapters();
                      app.UseEndpoints(endpoints =>
                      {
                          endpoints.Map("/{*url}", (context) => MyController(context));

                      });
                  });
          })
          .StartAsync();

        using var client = host.GetTestClient();

        // Act
        var result = await GetResponseAsync(client, endpoint);

        // Assert
        Assert.Equal(expected, result);
    }

    private static Task MyController(HttpContext context)
    {
        var session = context.Session;
        if (session == null)
        {
            context.Response.Write("Session:null");
        }
        else
        {
            context.Response.Write($"ReadOnly:{session.IsReadOnly}"); ;
        }

        return Task.CompletedTask;
    }

    private static async Task<string> GetResponseAsync(HttpClient client, string uri)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, UriKind.Relative));
        using var response = await client.SendAsync(message);

        return await response.Content.ReadAsStringAsync();
    }


    private static void SetSessionState(HttpContext context)
    {
        var path = context.Request.Path;

        switch (path)
        {
            case "/disabled":
                context.SetSessionStateBehavior(SessionStateBehavior.Disabled);
                break;
            case "/readonly":
                context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                break;
            case "/required":
                context.SetSessionStateBehavior(SessionStateBehavior.Required);
                break;
            case "/default":
                context.SetSessionStateBehavior(SessionStateBehavior.Default);
                break;
            default:
                break;
        }
    }
}
