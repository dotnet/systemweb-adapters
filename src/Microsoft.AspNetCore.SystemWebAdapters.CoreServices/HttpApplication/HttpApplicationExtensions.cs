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
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class HttpApplicationExtensions
{
    public static ISystemWebAdapterBuilder ConfigureApplication(this ISystemWebAdapterBuilder builder, Action<HttpApplicationOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddSingleton<HttpApplicationFactoryConfigureOptions>();
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

        builder.Services.TryAddTransient<IPostConfigureOptions<HttpApplicationOptions>, HttpApplicationFactoryConfigureOptions>();

        builder.Services.AddOptions<HttpApplicationOptions>()
            .Configure(configure);

        return builder;
    }

    public static ISystemWebAdapterBuilder AddHttpModule<TModule>(this ISystemWebAdapterBuilder builder)
        where TModule : class, IHttpModule
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureApplication(options =>
        {
            options.RegisterModule<TModule>();
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
        => builder.ApplicationServices.GetRequiredService<IOptions<HttpApplicationOptions>>().Value.IsHttpApplicationNeeded;

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
