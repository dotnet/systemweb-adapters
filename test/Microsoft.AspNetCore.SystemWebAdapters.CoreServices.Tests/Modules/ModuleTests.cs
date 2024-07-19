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
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModulesLibrary;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public abstract class ModuleTests(bool isBuffered)
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

    public static IEnumerable<object[]> GetAllEvents()
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

        foreach (var notification in all)
        {
            foreach (var mode in modes)
            {
                yield return new object[] { notification, mode };
            }
        }
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

        IEnumerable<ApplicationEvent> GetExpected(ApplicationEvent notification)
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

    private IEnumerable<ApplicationEvent> GetNotificationsUpTo(ApplicationEvent notification, bool isThrowing = false)
    {
        return isBuffered
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

    private async Task<List<ApplicationEvent>> RunAsync(string action, ApplicationEvent @event, RegisterMode mode)
    {
        var notifier = new NotificationCollection();
        var module = isBuffered ? typeof(BufferedTestModule) : typeof(NotBufferedTestModule);

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
                                if (mode == RegisterMode.Options)
                                {
                                    options.RegisterModule(module);
                                }

                                options.ArePreSendEventsEnabled = true;
                            });

                    })
                    .Configure(app =>
                    {
                        if (mode == RegisterMode.RegisterModule)
                        {
                            HttpApplication.RegisterModule(module);
                        }
                        else if (mode == RegisterMode.RegisterModuleOnStartup)
                        {
                            app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
                            {
                                HttpApplication.RegisterModule(module);
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

    private sealed class NotBufferedTestModule : BufferedTestModule
    {
        public override void Init(HttpApplication application)
        {
            application.BeginRequest += (s, o) => ((HttpApplication)s!).Context.Response.BufferOutput = false;
            base.Init(application);
        }
    }

    private class BufferedTestModule : EventsModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            context.AsAspNetCore().Features.GetRequired<NotificationCollection>().Add(Enum.Parse<ApplicationEvent>(name, ignoreCase: false));
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
