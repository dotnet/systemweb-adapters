// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder AddHttpApplication(this ISystemWebAdapterBuilder builder)
        => builder.AddHttpApplication(_ => { });

    public static ISystemWebAdapterBuilder AddHttpApplication(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<ModuleCollection>();
        builder.Services.TryAddTransient<IModuleRegistrar>(sp => sp.GetRequiredService<ModuleCollection>());
        builder.Services.TryAddSingleton<HttpApplicationPooledObjectPolicy>();
        builder.Services.TryAddSingleton<IPooledObjectPolicy<HttpApplication>>(sp => sp.GetRequiredService<HttpApplicationPooledObjectPolicy>());
        builder.Services.TryAddSingleton<ObjectPool<HttpApplication>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HttpApplicationOptions>>();
            var policy = sp.GetRequiredService<IPooledObjectPolicy<HttpApplication>>();

            var provider = new DefaultObjectPoolProvider
            {
                MaximumRetained = options.Value.PoolSize,
            };

            return provider.Create(policy);
        });

        builder.Services.AddOptions<HttpApplicationOptions>()
            .Configure<ModuleCollection>((options, modules) =>
            {
                options.ModuleCollection = modules;
            })
            .Configure(configure);

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpApplication<TApp>(this ISystemWebAdapterBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddHttpApplication(options =>
        {
            options.ApplicationType = typeof(TApp);
        });

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpApplication<TApp>(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions> configure)
        where TApp : HttpApplication
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddHttpApplication(options =>
        {
            options.ApplicationType = typeof(TApp);

            configure(options);
        });

        return builder;
    }

    internal static void UseHttpApplicationEvent(this IApplicationBuilder app, params ApplicationEvent[] preEvents)
        => app.UseHttpApplicationEvent(preEvents, Array.Empty<ApplicationEvent>());

    internal static void UseHttpApplicationEvent(this IApplicationBuilder app, ApplicationEvent[] preEvents, ApplicationEvent[] postEvents)
    {
        if (app.AreHttpApplicationEventsRequired())
        {
            app.Use(async (ctx, next) =>
            {
                var appFeature = ctx.Features.GetRequired<IHttpApplicationFeature>();
                var endFeature = ctx.Features.GetRequired<IHttpResponseEndFeature>();

                foreach (var @event in preEvents)
                {
                    await appFeature.RaiseEventAsync(@event);

                    if (endFeature.IsEnded)
                    {
                        return;
                    }
                }

                await next(ctx);

                foreach (var @event in postEvents)
                {
                    if (endFeature.IsEnded)
                    {
                        return;
                    }

                    await appFeature.RaiseEventAsync(@event);
                }
            });
        }
    }

    public static IApplicationBuilder UseAuthenticationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.HasBeenAdded())
        {
            return app;
        }

        app.UseSystemWebAdapterFeatures();
        app.UseHttpApplicationEvent(ApplicationEvent.AuthenticateRequest, ApplicationEvent.PostAuthenticateRequest);

        return app;
    }

    public static IApplicationBuilder UseAuthorizationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.HasBeenAdded())
        {
            return app;
        }

        app.UseSystemWebAdapterFeatures();
        app.UseAuthenticationEvents();
        app.UseHttpApplicationEvent(ApplicationEvent.AuthorizeRequest, ApplicationEvent.PostAuthorizeRequest);

        return app;
    }

    internal static bool AreHttpApplicationEventsRequired(this IApplicationBuilder builder)
        => builder.ApplicationServices.GetRequiredService<IOptions<HttpApplicationOptions>>().Value.IsAdded;
}
