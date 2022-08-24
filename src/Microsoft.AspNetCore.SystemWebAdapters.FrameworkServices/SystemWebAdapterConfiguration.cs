// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class SystemWebAdapterConfiguration
{
    private const string Key = "system-web-adapter";

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this HttpApplication application)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        application.Application.Lock();
        try
        {
            if (application.Application[Key] is null)
            {
                var services = new ServiceCollection();

                services.AddLogging();

                application.Application[Key] = new SystemWebAdapterBuilder(services);

                // If a service provider has been created, ensure it's disposed at
                // application shutdown.
                application.Disposed += (sender, args) =>
                {
                    if (application.Application[Key] is ServiceProvider serviceProvider)
                    {
                        serviceProvider.Dispose();
                    }
                };
            }

            if (application.Application[Key] is ISystemWebAdapterBuilder builder)
            {
                return builder;
            }
            else
            {
                throw new InvalidOperationException("SystemWebAdapter cannot be configured after its service provider is built and in use");
            }
        }
        finally
        {
            application.Application.UnLock();
        }
    }

    public static ISystemWebAdapterBuilder AddProxySupport(this ISystemWebAdapterBuilder builder, Action<ProxyOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddScoped<IHttpModule, ProxyHeaderModule>();
        builder.Services.AddOptions<ProxyOptions>()
            .Configure(configure);

        return builder;
    }

    internal static IServiceProvider? GetServiceProvider(this HttpApplicationState state)
    {
        if (state[Key] is ISystemWebAdapterBuilder)
        {
            Build(state);
        }

        return state[Key] as IServiceProvider;
    }

    private static void Build(HttpApplicationState state)
    {
        state.Lock();
        try
        {
            if (state[Key] is ISystemWebAdapterBuilder builder)
            {
                state[Key] = builder.Services.BuildServiceProvider();
            }
        }
        finally
        {
            state.UnLock();
        }
    }
}
