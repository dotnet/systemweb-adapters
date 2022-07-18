// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class SystemWebAdaptersExtensions
{
    private const string BuilderKey = "system-web-adapter-builder";
    private const string ServicesKey = "system-web-adapter-services";

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this HttpApplication application)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (application.Application[BuilderKey] is not ISystemWebAdapterBuilder builder)
        {
            builder = new SystemWebAdapterBuilder(new ServiceCollection());
            application.Application[BuilderKey] = builder;
        }

        // If a service provider has been created, ensure it's disposed at
        // application shutdown.
        application.Disposed += (sender, args) =>
            (application.Application[ServicesKey] as ServiceProvider)?.Dispose();

        return builder;
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
        var serviceProvider = state[ServicesKey] as IServiceProvider;

        if (serviceProvider is null)
        {
            state.Lock();
            try
            {
                serviceProvider = state[ServicesKey] as IServiceProvider;
                if (serviceProvider is null)
                {
                    var builder = state[BuilderKey] as ISystemWebAdapterBuilder;
                    state[ServicesKey] = serviceProvider = builder?.Services.BuildServiceProvider();
                }
            }
            finally
            {
                state.UnLock();
            }
        }

        return serviceProvider;
    }
}
