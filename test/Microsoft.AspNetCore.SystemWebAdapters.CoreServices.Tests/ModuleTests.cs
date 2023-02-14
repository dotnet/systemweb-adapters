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
    [InlineData("")]
    [InlineData("LogRequest")]
    [InlineData("PostLogRequest")]
    [InlineData("EndRequest")]
    [InlineData("PreSendRequestHeaders")]
    [Theory]
    public async Task AllModuleEvents(string notification)
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
        var result = await RunAsync<EventsModule>($"/?action=end&notification={notification}");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndBeginRequest()
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

    [Fact]
    public async Task CallEndAuthenticateRequest()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=AuthenticateRequest");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostAuthenticateRequest()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostAuthenticateRequest");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndAuthorizeRequest()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            AuthorizeRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=AuthorizeRequest");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostAuthorizeRequest()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            AuthorizeRequest
            PostAuthorizeRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostAuthorizeRequest");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndResolveRequestCache()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            AuthorizeRequest
            PostAuthorizeRequest
            ResolveRequestCache
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=ResolveRequestCache");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostResolveRequestCache()
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            PostAuthenticateRequest
            AuthorizeRequest
            PostAuthorizeRequest
            ResolveRequestCache
            PostResolveRequestCache
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostResolveRequestCache");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndMapRequestHandler()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=MapRequestHandler");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostMapRequestHandler()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostMapRequestHandler");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndAcquireRequestState()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=AcquireRequestState");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostAcquireRequestState()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostAcquireRequestState");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPreRequestHandlerExecute()
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

        var result = await RunAsync<EventsModule>("/?action=end&notification=PreRequestHandlerExecute");

        Assert.Equal(Expected, result);
    }

    [Fact(Skip = "Names don't match")]
    public async Task CallEndPostRequestHandlerExecute()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostRequestHandlerExecute");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndUpdateRequestCache()
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>("/?action=end&notification=UpdateRequestCache");

        Assert.Equal(Expected, result);
    }

    [Fact]
    public async Task CallEndPostUpdateRequestCache()
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

        var result = await RunAsync<EventsModule>("/?action=end&notification=PostUpdateRequestCache");

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
