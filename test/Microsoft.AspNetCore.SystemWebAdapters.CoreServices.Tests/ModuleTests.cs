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
using ModulesLibrary;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

public class ModuleTests
{
    [Fact]
    public async Task AllModuleEvents()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            AuthorizeRequest
            PostAuthorizeRequest
            ResolveRequestCache
            PostResolveRequestCache
            MapRequestHandler
            PostMapRequestHandler
            AcquireRequestState
            PostAcquireRequestState
            PreRequestHandlerExecute
            PostRequestHandlerExecute
            ReleaseRequestState
            PostReleaseRequestState
            UpdateRequestCache
            PostUpdateRequestCache
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;
        var result = await RunAsync<EventsModule>("/");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndInBeginModule()
    {
        const string Expected = """
            BeginRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=BeginRequest");

        Assert.Equal(Expected, result);
    }

    private static async Task<string> RunAsync<TModule>(string url)
        where TModule : class, IHttpModule
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
                            .AddHttpModule<TModule>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseRaiseAuthenticationEvents();
                        app.UseRaiseAuthorizationEvents();
                        app.UseSystemWebAdapters();

                        app.Run(ctx => Task.CompletedTask);
                    });
            })
            .StartAsync();

        using var response = await host.GetTestClient().GetAsync(new Uri(url, UriKind.Relative));

        var result = await response.Content.ReadAsStringAsync();

        return result.Trim();
    }
}
