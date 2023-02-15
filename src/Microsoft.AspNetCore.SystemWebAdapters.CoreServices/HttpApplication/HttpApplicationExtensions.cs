// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
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

    internal static void UseResponseEndShortCircuit(this IApplicationBuilder app)
    {
        app.Use((ctx, next) =>
        {
            if (ctx.Features.Get<IHttpResponseEndFeature>() is { IsEnded: true })
            {
                return Task.CompletedTask;
            }

            return next(ctx);
        });
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
        if (app.IsHttpApplicationRegistered())
        {
            app.UseMiddleware<SetHttpApplicationMiddleware>();
            app.UseHttpApplicationEvent((f, token) => f.RaiseBeginRequestAsync(token));
        }
    }

    internal static void UsePostHttpApplicationEvent(this IApplicationBuilder app, Func<IHttpApplicationEventsFeature, CancellationToken, ValueTask> appEvent)
    {
        if (app.IsHttpApplicationRegistered())
        {
            app.Use(async (ctx, next) =>
            {
                await next(ctx);

                if (ctx.Features.Get<IHttpResponseEndFeature>() is not { IsEnded: true } &&
                    ctx.Features.Get<IHttpApplicationEventsFeature>() is { } feature)
                {
                    await appEvent(feature, ctx.RequestAborted);
                }
            });
        }
    }

    internal static void UseHttpApplicationEvent(this IApplicationBuilder app, Func<IHttpApplicationEventsFeature, CancellationToken, ValueTask> appEvent)
    {
        if (app.IsHttpApplicationRegistered())
        {
            app.Use(async (ctx, next) =>
            {
                if (ctx.Features.Get<IHttpApplicationEventsFeature>() is { } feature)
                {
                    await appEvent(feature, ctx.RequestAborted);

                    if (ctx.Features.Get<IHttpResponseEndFeature>() is { IsEnded: true })
                    {
                        return;
                    }
                }

                await next(ctx);
            });
        }
    }

    public static IApplicationBuilder UseRaiseAuthenticationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSystemWebAdapterFeatures();
        app.UseHttpApplicationEvent((e, token) => e.RaiseAuthenticateRequestAsync(token));
        app.UseHttpApplicationEvent((e, token) => e.RaisePostAuthenticateRequestAsync(token));

        return app;
    }

    public static IApplicationBuilder UseRaiseAuthorizationEvents(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSystemWebAdapterFeatures();
        app.UseHttpApplicationEvent((e, token) => e.RaiseAuthorizeRequestAsync(token));
        app.UseHttpApplicationEvent((e, token) => e.RaisePostAuthorizeRequestAsync(token));

        return app;
    }

    internal static bool IsHttpApplicationRegistered(this IApplicationBuilder builder)
        => builder.ApplicationServices.GetRequiredService<IOptions<HttpApplicationOptions>>().Value.IsHttpApplicationNeeded;
}
