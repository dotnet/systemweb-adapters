// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder ConfigureApplication(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<IHttpApplicationFactory, HttpApplicationFactory>();
        builder.Services.TryAddTransient<HttpApplicationPolicy>();
        builder.Services.TryAddSingleton<ObjectPool<HttpApplication>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HttpApplicationOptions>>();
            var policy = sp.GetRequiredService<HttpApplicationPolicy>();

            var provider = new DefaultObjectPoolProvider
            {
                MaximumRetained = options.Value.PoolSize,
            };

            return provider.Create(policy);
        });

        builder.Services.AddOptions<HttpApplicationOptions>()
            .Configure(configure)
            .PostConfigure(c => c.MakeReadOnly());

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpModule<TModule>(this ISystemWebAdapterBuilder builder, string name)
        where TModule : class, IHttpModule
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureApplication(options =>
        {
            options.RegisterModule<TModule>(name);
        });

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpApplication<TApp>(this ISystemWebAdapterBuilder builder)
        where TApp : HttpApplication
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureApplication(options =>
        {
            options.ApplicationType = typeof(TApp);
        });

        return builder;
    }

    internal static void UseHttpApplication(this IApplicationBuilder app)
    {
        if (app.AreHttpApplicationEventsRequired())
        {
            app.UseMiddleware<HttpApplicationMiddleware>();
            app.UseHttpApplicationEvent(ApplicationEvent.BeginRequest);
        }
    }

    internal static void UseHttpApplicationEvent(this IApplicationBuilder app, params ApplicationEvent[] events)
        => app.UseHttpApplicationEvent(events, Array.Empty<ApplicationEvent>());

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

    public static IApplicationBuilder UseRaiseAuthenticationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSystemWebAdapterFeatures();
        app.UseHttpApplicationEvent(ApplicationEvent.AuthenticateRequest, ApplicationEvent.PostAuthenticateRequest);

        return app;
    }

    public static IApplicationBuilder UseRaiseAuthorizationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSystemWebAdapterFeatures();
        app.UseHttpApplicationEvent(ApplicationEvent.AuthorizeRequest, ApplicationEvent.PostAuthorizeRequest);

        return app;
    }

    internal static bool AreHttpApplicationEventsRequired(this IApplicationBuilder builder)
    {
        const string AreHttpApplicationEventsRequired = nameof(AreHttpApplicationEventsRequired);

        if (builder.Properties.TryGetValue(AreHttpApplicationEventsRequired, out var existing) && existing is bool b)
        {
            return b;
        }

        var options = builder.ApplicationServices.GetRequiredService<IOptions<HttpApplicationOptions>>().Value;

        var hasModules = options.Modules.Count > 0;
        var hasCustomApplication = options.ApplicationType != typeof(HttpApplication);

        var areEventsRequired = hasModules || hasCustomApplication;

        builder.Properties[AreHttpApplicationEventsRequired] = areEventsRequired;

        return areEventsRequired;
    }
}
