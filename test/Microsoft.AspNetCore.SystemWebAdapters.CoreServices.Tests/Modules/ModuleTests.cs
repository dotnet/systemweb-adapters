// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
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
    private static readonly string[] Initial = new[]
    {
        nameof(HttpApplication.BeginRequest),
        nameof(HttpApplication.AuthenticateRequest),
        nameof(HttpApplication.PostAuthenticateRequest),
        nameof(HttpApplication.AuthorizeRequest),
        nameof(HttpApplication.PostAuthorizeRequest),
        nameof(HttpApplication.ResolveRequestCache),
        nameof(HttpApplication.PostResolveRequestCache),
        nameof(HttpApplication.MapRequestHandler),
        nameof(HttpApplication.PostMapRequestHandler),
        nameof(HttpApplication.AcquireRequestState),
        nameof(HttpApplication.PostAcquireRequestState),
        nameof(HttpApplication.PreRequestHandlerExecute),
        nameof(HttpApplication.PostRequestHandlerExecute),
        nameof(HttpApplication.ReleaseRequestState),
        nameof(HttpApplication.PostReleaseRequestState),
        nameof(HttpApplication.UpdateRequestCache),
        nameof(HttpApplication.PostUpdateRequestCache),
    };

    private static readonly string[] Always = new[]
    {
        nameof(HttpApplication.LogRequest),
        nameof(HttpApplication.PostLogRequest),
        nameof(HttpApplication.EndRequest),
        nameof(HttpApplication.PreSendRequestHeaders),
    };

    public static IEnumerable<object[]> GetAllEvents()
        => Initial.Concat(Always).Select(o => new[] { o });

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task EndModuleEarly(string notification)
    {
        var expected = GetNotificationsUpTo(notification);
        var result = await RunAsync(ModuleTestModule.End, notification);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task CompleteModuleEarly(string notification)
    {
        var expected = GetNotificationsUpTo(notification);
        var result = await RunAsync(ModuleTestModule.Complete, notification);

        Assert.Equal(expected, result);
    }

    [MemberData(nameof(GetAllEvents))]
    [Theory]
    public async Task ModulesThrow(string notification)
    {
        var expected = GetExpected(notification).ToList();
        var result = await RunAsync(ModuleTestModule.Throw, notification);

        Assert.Equal(expected, result);

        static IEnumerable<string> GetExpected(string notification)
        {
            foreach (var item in GetNotificationsUpTo(notification))
            {
                yield return item;

                if (string.Equals(item, notification, StringComparison.Ordinal))
                {
                    yield return nameof(HttpApplication.Error);
                }
            }
        }
    }

    private static IEnumerable<string> GetNotificationsUpTo(string notification)
    {
        foreach (var n in Initial)
        {
            yield return n;

            if (string.Equals(n, notification, StringComparison.Ordinal))
            {
                break;
            }
        }

        foreach (var n in Always)
        {
            yield return n;
        }
    }

    private static async Task<List<string>> RunAsync(string action, string eventName)
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
                        services.AddRouting();
                        services.AddSystemWebAdapters()
                            .AddHttpApplication(options =>
                            {
                                options.RegisterModule<ModuleTestModule>();
                            });

                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.Use(async (ctx, next) =>
                        {
                            ctx.Features.Set(notifier);
                            try
                            {
                                await next(ctx);
                            }
                            catch (InvalidOperationException) when (action == ModuleTestModule.Throw)
                            {
                            }
                        });
                        app.UseAuthenticationEvents();
                        app.UseAuthorizationEvents();
                        app.UseSystemWebAdapters();

                        app.Run(ctx => Task.CompletedTask);
                    });
            })
            .StartAsync();

        var url = $"/?action={action}&notification={eventName}";

        using var _ = await host.GetTestClient().GetAsync(new Uri(url, UriKind.Relative));

        return notifier;
    }

    private sealed class NotificationCollection : List<string>
    {
    }

    private sealed class ModuleTestModule : EventsModule
    {
        protected override void InvokeEvent(HttpContext context, string name)
        {
            Add(context, name);
            base.InvokeEvent(context, name);
        }

        private static void Add(HttpContextCore context, string name)
        {
            context.Features.GetRequired<NotificationCollection>().Add(name);
        }
    }
}
