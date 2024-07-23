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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModulesLibrary;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class ModuleTests
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
        IEnumerable<ApplicationEvent> all = [.. BeforeHandlerEvents, .. AfterHandlerEvents, .. EndEvents, ApplicationEvent.PreSendRequestHeaders];

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
        var result = await RunAsync(ModuleTestModule.End, notification, mode);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task CompleteModuleEarly(ApplicationEvent notification, RegisterMode mode)
    {
        var expected = GetNotificationsUpTo(notification);
        var result = await RunAsync(ModuleTestModule.Complete, notification, mode);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task ModulesThrow(ApplicationEvent notification, RegisterMode mode)
    {
        var expected = GetExpected(notification).ToList();
        var result = await RunAsync(ModuleTestModule.Throw, notification, mode);

        Assert.Equal(expected, result);

        static IEnumerable<ApplicationEvent> GetExpected(ApplicationEvent notification)
        {
            foreach (var item in GetNotificationsUpTo(notification))
            {
                yield return item;

                if (item == notification)
                {
                    yield return ApplicationEvent.Error;
                }
            }
        }
    }

    private static IEnumerable<ApplicationEvent> GetNotificationsUpTo(ApplicationEvent notification)
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
                                if (mode == RegisterMode.Options)
                                {
                                    options.RegisterModule<ModuleTestModule>();
                                }
                            });

                    })
                    .Configure(app =>
                    {
                        if (mode == RegisterMode.RegisterModule)
                        {
                            HttpApplication.RegisterModule(typeof(ModuleTestModule));
                        }
                        else if (mode == RegisterMode.RegisterModuleOnStartup)
                        {
                            app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
                            {
                                HttpApplication.RegisterModule(typeof(ModuleTestModule));
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

    [Fact]
    public async Task PreSendEventThrownIfNotBuffering()
    {
        // Arrange
        IEnumerable<ApplicationEvent> expected =
        [
            .. BeforeHandlerEvents,
            ApplicationEvent.PreSendRequestHeaders,
            .. AfterHandlerEvents,
            .. EndEvents
        ];

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
                        services.AddRouting();
                        services.AddSystemWebAdapters()
                            .AddHttpApplication(options =>
                            {
                                options.RegisterModule<ModulePreSendHeaders>();
                            });

                    })
                    .Configure(app =>
                    {
                        app.Use((ctx, next) =>
                        {
                            ctx.Features.Set(notifier);
                            return next(ctx);
                        });
                        app.UseRouting();

                        app.UseSystemWebAdapters();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.Map("/", (HttpContextCore ctx) =>
                            {
                                ctx.Features.GetRequiredFeature<IHttpResponseBodyFeature>().DisableBuffering();

                                var systemWeb = ctx.AsSystemWeb();
                                systemWeb.Response.Write(systemWeb.CurrentNotification);
                                systemWeb.Response.Write(" ");
                                systemWeb.Response.Output.Flush();
                                systemWeb.Response.Write(systemWeb.CurrentNotification);
                                systemWeb.Response.Output.Flush();
                            });
                        });
                    });
            })
            .StartAsync();

        // Act
        var result = await host.GetTestClient().GetStringAsync(new Uri("/", UriKind.Relative));

        // Assert
        Assert.Equal(expected, notifier);
        Assert.Equal(result, $"{ApplicationEvent.ExecuteRequestHandler} {ApplicationEvent.ExecuteRequestHandler}");
    }

    private sealed class NotificationCollection : List<ApplicationEvent>
    {
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
                    catch (InvalidOperationException) when (_action == ModuleTestModule.Throw)
                    {
                    }
                });

                next(builder);
            };
    }

    private sealed class ModulePreSendHeaders : BaseModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            context.AsAspNetCore().Features.GetRequiredFeature<NotificationCollection>().Add(Enum.Parse<ApplicationEvent>(name, ignoreCase: false));
        }
    }

    private sealed class ModuleTestModule : EventsModule
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
