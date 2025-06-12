// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public static class SystemWebAdapterExtensions
{
    public static HttpApplicationHostBuilder RegisterWebObjectActivator(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddHostedService<WebObjectActivatorHostServices>();

        return builder;
    }

    public static ISystemWebAdapterBuilder AddSystemAdapters(this IServiceCollection services)
       => new SystemWebAdapterBuilder(services);

    private sealed class WebObjectActivatorHostServices : IServiceProvider, IHostedService
    {
        private readonly IServiceProvider _services;

        public WebObjectActivatorHostServices(IServiceProvider services)
        {
            if (HttpRuntime.WebObjectActivator is { })
            {
                throw new InvalidOperationException("HttpRuntime.WebObjectActivator is already configured");
            }

            _services = services;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return _services;
            }
            else if (serviceType == typeof(IKeyedServiceProvider) && _services is IKeyedServiceProvider keyed)
            {
                return keyed;
            }
            else if (_services.GetService(serviceType) is { } known)
            {
                return known;
            }
            else if (serviceType.IsAbstract)
            {
                return null;
            }

            return CreateNonPublicInstance(serviceType);

            // The implementation of dependency injection in System.Web expects to be able to create instances
            // of non-public and unregistered types.
            static object CreateNonPublicInstance(Type serviceType) => Activator.CreateInstance(
                serviceType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null);
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
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
