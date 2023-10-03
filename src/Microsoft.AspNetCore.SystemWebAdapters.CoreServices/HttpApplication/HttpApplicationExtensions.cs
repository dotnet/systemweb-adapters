// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder AddHttpApplication(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<HttpApplicationPooledObjectPolicy>();
        builder.Services.AddTransient<IStartupFilter, HttpApplicationStartupFilter>();
        builder.Services.TryAddSingleton<IPooledObjectPolicy<HttpApplication>>(ctx => ctx.GetRequiredService<HttpApplicationPooledObjectPolicy>());
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
            .Configure(configure)
            .PostConfigure(c => c.MakeReadOnly());

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

    private sealed class HttpApplicationStartupFilter : IStartupFilter
    {
        private readonly ObjectPool<HttpApplication> _pool;

        public HttpApplicationStartupFilter(ObjectPool<HttpApplication> pool)
        {
            _pool = pool;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => builder =>
        {
            CallStartup(builder.ApplicationServices);
            next(builder);
        };

        private void CallStartup(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var app = _pool.Get();

            // ASP.NET Framework provided an HttpContext instance that was not tied to a request for Start
            app.Context = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider,
            };

            try
            {
                // This is only invoked at the beginning of the application
                // See https://referencesource.microsoft.com/#System.Web/HttpApplication.cs,2417
                app.InvokeEvent(ApplicationEvent.ApplicationStart);
            }
            finally
            {
                _pool.Return(app);
            }
        }
    }
}
