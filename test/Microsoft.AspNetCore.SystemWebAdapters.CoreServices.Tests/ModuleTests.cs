// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
    [InlineData("End", "")]
    [InlineData("End", "LogRequest")]
    [InlineData("End", "PostLogRequest")]
    [InlineData("End", "EndRequest")]
    [InlineData("End", "PreSendRequestHeaders")]
    [InlineData("Complete", "")]
    [InlineData("Complete", "LogRequest")]
    [InlineData("Complete", "PostLogRequest")]
    [InlineData("Complete", "CompleteRequest")]
    [InlineData("Complete", "PreSCompleteRequestHeaders")]
    [Theory]
    public async Task AllModuleEvents(string action, string notification)
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
        var result = await RunAsync<EventsModule>(action, notification);

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndBeginRequest(string action)
    {
        const string Expected = """
            BeginRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>(action, "BeginRequest");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndAuthenticateRequest(string action)
    {
        const string Expected = """
            BeginRequest
            AuthenticateRequest
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>(action, "AuthenticateRequest");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostAuthenticateRequest(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostAuthenticateRequest");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndAuthorizeRequest(string action)
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

        var result = await RunAsync<EventsModule>(action, "AuthorizeRequest");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostAuthorizeRequest(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostAuthorizeRequest");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndResolveRequestCache(string action)
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

        var result = await RunAsync<EventsModule>(action, "ResolveRequestCache");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostResolveRequestCache(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostResolveRequestCache");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndMapRequestHandler(string action)
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

        var result = await RunAsync<EventsModule>(action, "MapRequestHandler");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostMapRequestHandler(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostMapRequestHandler");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndAcquireRequestState(string action)
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

        var result = await RunAsync<EventsModule>(action, "AcquireRequestState");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostAcquireRequestState(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostAcquireRequestState");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPreRequestHandlerExecute(string action)
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
            LogRequest
            PostLogRequest
            EndRequest
            PreSendRequestHeaders
            """;

        var result = await RunAsync<EventsModule>(action, "PreRequestHandlerExecute");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostRequestHandlerExecute(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostRequestHandlerExecute");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndUpdateRequestCache(string action)
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

        var result = await RunAsync<EventsModule>(action, "UpdateRequestCache");

        Assert.Equal(Expected, result);
    }

    [InlineData("CompleteRequest")]
    [InlineData("End")]
    [Theory]
    public async Task CallEndPostUpdateRequestCache(string action)
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

        var result = await RunAsync<EventsModule>(action, "PostUpdateRequestCache");

        Assert.Equal(Expected, result);
    }

    private static async Task<string> RunAsync<TModule>(string action, string eventName)
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
                            .AddHttpApplication(options =>
                            {
                                options.RegisterModule<TModule>(typeof(TModule).Name);
                            });

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

        var url = $"/?action=end&notification={eventName}";
        using var response = await host.GetTestClient().GetAsync(new Uri(url, UriKind.Relative));

        var result = await response.Content.ReadAsStringAsync();

        return result.Trim();
    }
}
