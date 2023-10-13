// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.SessionState;
using Autofac.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
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
                        .WrapAspNetCoreSession();
                      services.AddDistributedMemoryCache();
                  })
                  .Configure(app =>
                  {
                      app.UseRouting();
                      app.Use((ctx, next) =>
                      {
                          SetOverrideSessionState(ctx);
                          return next(ctx);
                      });
                      app.UseSession();
                      app.UseSystemWebAdapters();
                      app.UseEndpoints(endpoints =>
                      {
                          endpoints.MapControllers();

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

    private static void SetOverrideSessionState(HttpContext context)
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
}

[ApiController]
public class SessionStatusController : ControllerBase
{
    [HttpGet]
    [Route("/")]
    public ActionResult<string> Get()
    {
        return Ok(GetSessionStatusString(System.Web.HttpContext.Current));
    }

    [HttpGet]
    [Session(SessionBehavior = SessionStateBehavior.Disabled)]
    [Route("/disabled")]
    public ActionResult<string> GetDisabled()
    {
        return Ok(GetSessionStatusString(System.Web.HttpContext.Current));
    }

    [HttpGet]
    [Session(SessionBehavior = SessionStateBehavior.ReadOnly)]
    [Route("/readonly")]
    public ActionResult<string> GetReadOnly()
    {
        return Ok(GetSessionStatusString(System.Web.HttpContext.Current));
    }

    [HttpGet]
    [Session(SessionBehavior = SessionStateBehavior.Required)]
    [Route("/required")]
    public ActionResult<string> GetRequired()
    {
        return Ok(GetSessionStatusString(System.Web.HttpContext.Current));
    }

    [HttpGet]
    [Session(SessionBehavior = SessionStateBehavior.Default)]
    [Route("/default")]
    public ActionResult<string> GetDefault()
    {
        return Ok(GetSessionStatusString(System.Web.HttpContext.Current));
    }

    public static string GetSessionStatusString(HttpContext context)
    {
        var session = context.Session;
        if (session == null)
        {
            return "Session:null";
        }
        else
        {
            return $"ReadOnly:{session.IsReadOnly}";
        }

    }
}

