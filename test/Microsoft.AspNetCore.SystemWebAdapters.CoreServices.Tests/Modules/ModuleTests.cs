// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModulesLibrary;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public abstract class ModuleTests<T>
{
    private static readonly ImmutableArray<ApplicationEvent> BeforeHandlerEvents =
    [
        ApplicationEvent.BeginRequest,
        ApplicationEvent.AuthenticateRequest,
        ApplicationEvent.PostAuthenticateRequest,
        ApplicationEvent.AuthorizeRequest,
        ApplicationEvent.PostAuthorizeRequest,
        ApplicationEvent.ResolveRequestCache,
        ApplicationEvent.PostResolveRequestCache,
        ApplicationEvent.MapRequestHandler,
        ApplicationEvent.PostMapRequestHandler,
        ApplicationEvent.AcquireRequestState,
        ApplicationEvent.PostAcquireRequestState,
        ApplicationEvent.PreRequestHandlerExecute,
    ];

    private static readonly ImmutableArray<ApplicationEvent> AfterHandlerEvents =
    [
        ApplicationEvent.PostRequestHandlerExecute,
        ApplicationEvent.ReleaseRequestState,
        ApplicationEvent.PostReleaseRequestState,
        ApplicationEvent.UpdateRequestCache,
        ApplicationEvent.PostUpdateRequestCache,
    ];

    private static readonly ImmutableArray<ApplicationEvent> EndEvents =
    [
        ApplicationEvent.LogRequest,
        ApplicationEvent.PostLogRequest,
        ApplicationEvent.EndRequest,
    ];

    private static bool IsBuffered
    {
        get
        {
            if (typeof(T) == typeof(NotBufferedModuleTests))
            {
                return false;
            }
            else if (typeof(T) == typeof(BufferedModuleTests))
            {
                return true;
            }
            else
            {
                throw new ArgumentOutOfRangeException(typeof(T).FullName);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Needed for xUnit theory tests")]
    public static TheoryData<ApplicationEvent, RegisterMode> GetAllEvents()
    {
        IEnumerable<ApplicationEvent> all =
        [
            .. BeforeHandlerEvents,
            .. AfterHandlerEvents,
            .. EndEvents,
            ApplicationEvent.PreSendRequestHeaders,
            ApplicationEvent.PreSendRequestContent
        ];

        var modes = Enum.GetValues<RegisterMode>();
        var data = new TheoryData<ApplicationEvent, RegisterMode>();

        foreach (var notification in all)
        {
            foreach (var mode in modes)
            {
                data.Add(notification, mode);
            }
        }

        return data;
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task EndModuleEarly(ApplicationEvent notification, RegisterMode mode)
    {
        var expected = GetNotificationsUpTo(notification);
        var result = await RunAsync(EventsModule.End, notification, mode);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task CompleteModuleEarly(ApplicationEvent notification, RegisterMode mode)
    {
        var expected = GetNotificationsUpTo(notification);
        var result = await RunAsync(EventsModule.Complete, notification, mode);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task ModulesThrow(ApplicationEvent notification, RegisterMode mode)
    {
        var expected = GetExpected(notification).ToList();
        var result = await RunAsync(EventsModule.Throw, notification, mode);

        Assert.Equal(expected, result);

        static IEnumerable<ApplicationEvent> GetExpected(ApplicationEvent notification)
        {
            foreach (var item in GetNotificationsUpTo(notification, isThrowing: true))
            {
                yield return item;

                if (item == notification)
                {
                    yield return ApplicationEvent.Error;
                }
            }
        }
    }

    [Fact]
    public async Task PreSendRequestHeadersAddHeaders()
    {
        // Arrange
        const string HeaderName = "name";
        const string HeaderValue = "value";
        const string Result = "Hello world!";

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddSystemWebAdapters()
                        .AddHttpApplication(options =>
                        {
                            options.RegisterModule<BufferToggleModule>();
                            options.RegisterModule<PreSendHeadersAddHeaderModule>();
                            options.ArePreSendEventsEnabled = true;
                        });

                })
                .Configure(app =>
                {
                    app.Use((ctx, next) =>
                    {
                        ctx.Features.Set(new ResultHeader { { HeaderName, HeaderValue } });
                        return next(ctx);
                    });
                    app.UseSystemWebAdapters();

                    app.Run(ctx => ctx.Response.WriteAsync(Result));
                })).StartAsync();

        // Act
        using var response = await host.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        // Assert
        Assert.True(response.Headers.TryGetValues(HeaderName, out var resultHeader));
        Assert.Equal([HeaderValue], resultHeader);
        Assert.Equal(Result, await response.Content.ReadAsStringAsync());
    }

    private sealed class PreSendHeadersAddHeaderModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            application.PreSendRequestHeaders += (s, o) =>
            {
                if (s is HttpApplication { Context: { } context })
                {
                    foreach (var (name, value) in context.AsAspNetCore().Features.GetRequiredFeature<ResultHeader>())
                    {
                        context.Response.AddHeader(name, value);
                    }
                }
            };
        }
    }

    private sealed class ResultHeader : Dictionary<string, string>
    {
    }

    private static IEnumerable<ApplicationEvent> GetNotificationsUpTo(ApplicationEvent notification, bool isThrowing = false)
    {
        return IsBuffered
                ? GetExpectedBufferedNotificationsUntilAction(notification, isThrowing)
                : GetExpectedUnbufferedNotificationsUntilAction(notification, isThrowing);

        // When the stream is buffered, we expect the notifications to be grouped mostly together and the PreSend* events are sent at the end
        static IEnumerable<ApplicationEvent> GetExpectedBufferedNotificationsUntilAction(ApplicationEvent notification, bool isThrowing = false)
        {
            IEnumerable<ApplicationEvent> initial = [.. BeforeHandlerEvents, .. AfterHandlerEvents];

            foreach (var n in initial)
            {
                yield return n;

                if (n == notification)
                {
                    break;
                }
            }

            foreach (var n in EndEvents)
            {
                yield return n;
            }

            yield return ApplicationEvent.PreSendRequestHeaders;

            var expectsFinalPreSendRequestContent = !(isThrowing && notification is ApplicationEvent.PreSendRequestHeaders);

            if (expectsFinalPreSendRequestContent)
            {
                yield return ApplicationEvent.PreSendRequestContent;
            }
        }

        // When the stream is unbuffered, the PreSend* events will occur during each event because we we cause data to be flushed for the test
        static IEnumerable<ApplicationEvent> GetExpectedUnbufferedNotificationsUntilAction(ApplicationEvent notification, bool isThrowing)
        {
            var remaining = EndEvents;
            var bufferedEvents = GetExpectedBufferedNotificationsUntilAction(notification, isThrowing)
                .Where(e => e is not ApplicationEvent.PreSendRequestHeaders)
                .Where(e => e is not ApplicationEvent.PreSendRequestContent);
            var hasSentPreSendHeader = false;

            foreach (var appEvent in bufferedEvents)
            {
                yield return appEvent;

                remaining = remaining.Remove(appEvent);

                if (!hasSentPreSendHeader)
                {
                    hasSentPreSendHeader = true;
                    yield return ApplicationEvent.PreSendRequestHeaders;

                    if (notification is ApplicationEvent.PreSendRequestHeaders)
                    {
                        break;
                    }
                }

                if (notification != ApplicationEvent.PreSendRequestHeaders || !isThrowing)
                {
                    yield return ApplicationEvent.PreSendRequestContent;
                }

                if (notification is ApplicationEvent.PreSendRequestContent)
                {
                    break;
                }
            }

            foreach (var r in remaining)
            {
                yield return r;
            }

            var expectsFinalPreSendRequestContent = !(isThrowing && notification is ApplicationEvent.PreSendRequestContent or ApplicationEvent.PreSendRequestHeaders);

            if (!remaining.IsEmpty && expectsFinalPreSendRequestContent)
            {
                yield return ApplicationEvent.PreSendRequestContent;
            }
        }
    }

    private static async Task<List<ApplicationEvent>> RunAsync(string action, ApplicationEvent @event, RegisterMode mode)
    {
        var notifier = new NotificationCollection();

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
                        services.AddSingleton<IStartupFilter>(new ModuleTestStartup(notifier, action));
                        services.AddRouting();
                        services.AddSystemWebAdapters()
                            .AddHttpApplication(options =>
                            {
                                options.RegisterModule<BufferToggleModule>();

                                if (mode == RegisterMode.Options)
                                {
                                    options.RegisterModule<NotificationTrackingModule>();
                                }

                                options.ArePreSendEventsEnabled = true;
                            });

                    })
                    .Configure(app =>
                    {
                        if (mode == RegisterMode.RegisterModule)
                        {
                            HttpApplication.RegisterModule(typeof(NotificationTrackingModule));
                        }
                        else if (mode == RegisterMode.RegisterModuleOnStartup)
                        {
                            app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
                            {
                                HttpApplication.RegisterModule(typeof(NotificationTrackingModule));
                            });
                        }

                        app.UseRouting();

                        app.UseAuthenticationEvents();
                        app.UseAuthorizationEvents();
                        app.UseSystemWebAdapters();

                        app.Run(ctx => Task.CompletedTask);
                    });
            })
            .StartAsync();

        var url = $"/?action={action}&notification={@event}";

        try
        {
            using var _ = await host.GetTestClient().GetAsync(new Uri(url, UriKind.Relative));
        }
        finally
        {
            await host.StopAsync();
        }

        return notifier;
    }
    private sealed class NotificationCollection : List<ApplicationEvent>
    {
        public new void Add(ApplicationEvent appEvent)
        {
            // Prevent duplicate PreSendRequestContent since we can't really control when the Flush is called so we just want to track that at least one occurs with the test
            if (appEvent != ApplicationEvent.PreSendRequestContent || this[^1] != ApplicationEvent.PreSendRequestContent)
            {
                base.Add(appEvent);
            }
        }
    }

    private sealed class ModuleTestStartup : IStartupFilter
    {
        private readonly NotificationCollection _collection;
        private readonly string _action;

        public ModuleTestStartup(NotificationCollection collection, string action)
        {
            _collection = collection;
            _action = action;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                builder.Use(async (ctx, next) =>
                {
                    ctx.Features.Set(_collection);

                    try
                    {
                        await next(ctx);
                    }
                    catch (InvalidOperationException) when (_action == EventsModule.Throw)
                    {
                    }
                });

                next(builder);
            };
    }

    /// <summary>
    /// Module used to toggle buffer output depending on the test suite we're running
    /// </summary>
    private sealed class BufferToggleModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += (s, o) => ((HttpApplication)s!).Context.Response.BufferOutput = IsBuffered;
        }
    }

    private class NotificationTrackingModule : EventsModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            context.AsAspNetCore().Features.GetRequiredFeature<NotificationCollection>().Add(Enum.Parse<ApplicationEvent>(name, ignoreCase: false));
            base.InvokeEvent(context, name);
        }
    }

    public enum RegisterMode
    {
        Options,
        RegisterModule,
        RegisterModuleOnStartup,
    }
}
