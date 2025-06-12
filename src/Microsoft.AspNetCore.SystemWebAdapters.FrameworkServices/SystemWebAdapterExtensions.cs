// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.SystemWebAdapters.AspireConstants;

namespace System.Web;

public static class SystemWebAdapterExtensions
{
    public static HttpApplicationHostBuilder RegisterWebJobActivator(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddHostedService<RegisteredHostServices>();

        return builder;
    }

    public static ISystemWebAdapterBuilder AddSystemWebAdapters(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var config = builder.Configuration;

        var adapters = builder.Services.AddSystemAdapters();

        if (config.GetValue<bool>(ProxyKey + IsEnabled))
        {
            adapters.AddProxySupport(config.GetSection(ProxyKey).Bind);
        }

        if (config.GetValue<bool>(RemoteKey + IsEnabled))
        {
            var remoteConfig = adapters.AddRemoteAppServer(config.GetSection(RemoteKey).Bind);

            if (config.GetValue<bool>(RemoteSessionKey + IsEnabled))
            {
                remoteConfig.AddSessionServer(config.GetSection(RemoteSessionKey).Bind);
            }

            if (config.GetValue<bool>(RemoteAuthKey + IsEnabled))
            {
                remoteConfig.AddAuthenticationServer(config.GetSection(RemoteAuthKey).Bind);
            }
        }

        return adapters;
    }

    private sealed class RegistrationOptions
    {
        public ProxyOptions? Proxy { get; set; }

        public RemoteAppServerOptions? Server { get; set; }
    }

    public static ISystemWebAdapterBuilder AddSystemAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);

    private sealed class RegisteredHostServices(IServiceProvider services, ILogger<RegisteredHostServices> logger) : IServiceProvider, IHostedService
    {
        public object? GetService(Type serviceType)
        {
            if (services.GetService(serviceType) is { } known)
            {
                return known;
            }

            if (!serviceType.IsAbstract)
            {
                return CreateNonPublicInstance(serviceType);
            }

            return null;

            // The implementation of dependency injection in System.Web expects to be able to create instances
            // of non-public and unregistered types.
            static object CreateNonPublicInstance(Type serviceType) => Activator.CreateInstance(
                serviceType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null);
        }

        [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            if (HttpRuntime.WebObjectActivator is { })
            {
                logger.LogCritical("WebObjectActivator is already set and will not be overriden");
            }

            HttpRuntime.WebObjectActivator = this;
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            HttpRuntime.WebObjectActivator = null;
            return Task.CompletedTask;
        }
    }
}
