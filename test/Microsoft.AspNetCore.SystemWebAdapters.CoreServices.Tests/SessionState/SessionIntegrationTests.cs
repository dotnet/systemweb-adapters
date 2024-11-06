// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests.SessionState;

[Collection(nameof(SelfHostedTests))]
public class SessionIntegrationTests
{
    [InlineData("/?override=disabled", "Session:null")]
    [InlineData("/?override=readonly", "ReadOnly:True")]
    [InlineData("/?override=required", "ReadOnly:False")]
    [InlineData("/?override=default", "Session:null")]
    [Theory]
    public async Task TestSetSessionStateBehavior(string endpoint, string expected)
    {
        var actual = await GetAsync(endpoint);
        Assert.Equal(expected, actual);
    }

    [InlineData("/disabled", "Session:null")]
    [InlineData("/readonly", "ReadOnly:True")]
    [InlineData("/session", "ReadOnly:False")]
    [InlineData("/required", "ReadOnly:False")]
    [InlineData("/default", "Session:null")]
    [Theory]
    public async Task TestSessionAttribute(string endpoint, string expected)
    {
        var actual = await GetAsync(endpoint);
        Assert.Equal(expected, actual);
    }

    [InlineData("/disabled?override=required", "ReadOnly:False")]
    [InlineData("/disabled?override=readonly", "ReadOnly:True")]
    [InlineData("/readonly?override=required", "ReadOnly:False")]
    [InlineData("/default?override=disabled", "Session:null")]
    [Theory]
    public async Task TestOverrideSessionStateBehavior(string endpoint, string expected)
    {
        var actual = await GetAsync(endpoint);
        Assert.Equal(expected, actual);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SessionStartEvent(bool abandon)
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
                       services.AddControllers();
                       services.AddSystemWebAdapters()
                         .AddHttpApplication<SessionApplication>()
                         .AddWrappedAspNetCoreSession();

                       services.AddDistributedMemoryCache();
                   })
                   .Configure(app =>
                   {
                       app.UseRouting();
                       app.UseSession();
                       app.UseSystemWebAdapters();

                       if (abandon)
                       {
                           app.Use((ctx, next) =>
                           {
                               ctx.AsSystemWeb().Session!.Abandon();
                               return next(ctx);
                           });
                       }

                       app.UseEndpoints(endpoints =>
                       {
                           endpoints.MapGet("/", ctx => Task.CompletedTask)
                            .RequireSystemWebAdapterSession();
                       });
                   });
           })
           .StartAsync();

        // Act
        var result = await host.GetTestClient().GetStringAsync(new Uri("/", UriKind.Relative));

        // Assert
        if (abandon)
        {
            Assert.Equal("SessionStart::SessionEnd", result);
        }
        else
        {

            Assert.Equal("SessionStart::", result);
        }

        await host.StopAsync();
    }

    private sealed class SessionApplication : HttpApplication
    {
        private void Session_Start(Object sender, EventArgs e)
        {
            HttpContext.Current?.Response.Write("SessionStart::");
        }

        private void Session_End(Object sender, EventArgs e)
        {
            HttpContext.Current?.Response.Write("SessionEnd");
        }
    }

    private static async Task<string> GetAsync(string endpoint)
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
                      services.AddControllers();
                      services.AddSystemWebAdapters()
                        .AddWrappedAspNetCoreSession();
                      services.AddDistributedMemoryCache();
                  })
                  .Configure(app =>
                  {
                      app.UseRouting();
                      app.Use((ctx, next) =>
                      {
                          SetOverrideSessionBehavior(ctx);
                          return next(ctx);
                      });
                      app.UseSession();
                      app.UseSystemWebAdapters();
                      app.UseEndpoints(endpoints =>
                      {
                          endpoints.MapGet("/", (context) => GetSessionStatus(context));
                          endpoints.MapGet("/session", (context) => GetSessionStatus(context)).RequireSystemWebAdapterSession();
                          endpoints.MapGet("/disabled", (context) => GetSessionStatus(context)).WithMetadata(new SessionAttribute { SessionBehavior = SessionStateBehavior.Disabled });
                          endpoints.MapGet("/readonly", (context) => GetSessionStatus(context)).WithMetadata(new SessionAttribute { SessionBehavior = SessionStateBehavior.ReadOnly });
                          endpoints.MapGet("/required", (context) => GetSessionStatus(context)).WithMetadata(new SessionAttribute { SessionBehavior = SessionStateBehavior.Required });
                          endpoints.MapGet("/default", (context) => GetSessionStatus(context)).WithMetadata(new SessionAttribute { SessionBehavior = SessionStateBehavior.Default });
                      });
                  });
          })
          .StartAsync();

        var uri = new Uri(endpoint, UriKind.Relative);

        try
        {
            return await host.GetTestClient().GetStringAsync(uri).ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static void SetOverrideSessionBehavior(HttpContext context)
    {
        string? overrideValue = context.Request.QueryString["override"];

        switch (overrideValue)
        {
            case "disabled":
                context.SetSessionStateBehavior(SessionStateBehavior.Disabled);
                break;
            case "readonly":
                context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
                break;
            case "required":
                context.SetSessionStateBehavior(SessionStateBehavior.Required);
                break;
            case "default":
                context.SetSessionStateBehavior(SessionStateBehavior.Default);
                break;
            default:
                break;
        }
    }

    private static Task GetSessionStatus(HttpContext context)
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
}
