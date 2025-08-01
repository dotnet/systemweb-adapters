// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public static class SystemWebAdapterConfiguration
{
    private const string Key = "system-web-adapter";

    [Obsolete("Prefer using HttpApplicationHost.RegisterHost(...) instead")]
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
                var builder = HttpApplicationHost.CreateBuilder();

                application.Application[Key] = builder;

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

            if (application.Application[Key] is HttpApplicationHostBuilder existing)
            {
                return existing.Services.AddSystemWebAdapters();
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

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);

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

    [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Lifetime managed with IIS lifecycle")]
    internal static void EnsureSystemWebAdapterBuilderBuilt(this HttpApplicationState state)
    {
        if (state[Key] is HttpApplicationHostBuilder)
        {
            if (Build(state) is { } builder)
            {
                builder.BuildAndRunInBackground();
            }
        }
    }

    private static HttpApplicationHostBuilder? Build(HttpApplicationState state)
    {
        state.Lock();
        try
        {
            if (state[Key] is HttpApplicationHostBuilder builder)
            {
                // Set it as a random object so it can be tracked that it was already built
                state[Key] = new object();

                return builder;
            }
        }
        finally
        {
            state.UnLock();
        }

        return null;
    }
}
