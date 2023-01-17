// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder AddHttpModule<TModule>(this ISystemWebAdapterBuilder builder)
        where TModule : class, IHttpModule
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpApplicationPool();
        builder.Services.AddTransient<IHttpModule, TModule>();
        builder.Services.TryAddTransient<IPooledObjectPolicy<HttpApplication>, HttpApplicationPolicy<HttpApplication>>();

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpApplication<TApp>(this ISystemWebAdapterBuilder builder)
        where TApp : HttpApplication
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHttpApplicationPool();
        builder.Services.AddTransient<IPooledObjectPolicy<HttpApplication>, HttpApplicationPolicy<TApp>>();

        return builder;
    }

    private static IServiceCollection AddHttpApplicationPool(this IServiceCollection services)
    {
        services.TryAddSingleton<HttpApplicationEventFactory>();
        services.TryAddSingleton<HttpApplicationState>(_ => new HttpApplicationState());
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton<ObjectPool<HttpApplication>>(sp =>
        {
            var provider = sp.GetRequiredService<ObjectPoolProvider>();
            var policy = sp.GetRequiredService<IPooledObjectPolicy<HttpApplication>>();
            return provider.Create(policy);
        });

        return services;
    }

    public static IApplicationBuilder UseRaiseAuthenticationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.IsHttpApplicationRegistered())
        {
            app.UseMiddleware<RaiseAuthenticateRequest>();
        }

        return app;
    }

    public static IApplicationBuilder UseRaiseAuthorizationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.IsHttpApplicationRegistered())
        {
            app.UseMiddleware<RaiseAuthorizeRequest>();
        }

        return app;
    }

    internal static bool IsHttpApplicationRegistered(this IApplicationBuilder builder)
        => builder.ApplicationServices.GetService<ObjectPool<HttpApplication>>() is not null;

    internal sealed class RaiseAuthenticateRequest : HttpApplicationEventsMiddleware
    {
        public RaiseAuthenticateRequest(RequestDelegate next)
            : base(next)
        {
        }

        protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContextCore context, IHttpApplicationEventsFeature events)
        {
            await events.RaiseAuthenticateRequestAsync(context.RequestAborted);
            await events.RaisePostAuthenticateRequestAsync(context.RequestAborted);
            await next(context);
        }
    }

    internal sealed class RaiseAuthorizeRequest : HttpApplicationEventsMiddleware
    {
        public RaiseAuthorizeRequest(RequestDelegate next)
            : base(next)
        {
        }

        protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContextCore context, IHttpApplicationEventsFeature events)
        {
            await events.RaiseAuthorizeRequestAsync(context.RequestAborted);
            await events.RaisePostAuthorizeRequestAsync(context.RequestAborted);
            await next(context);
        }
    }
}
