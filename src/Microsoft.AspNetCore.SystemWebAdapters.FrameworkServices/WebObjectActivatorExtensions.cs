// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace System.Web;

public static partial class WebObjectActivatorExtensions
{
    [Obsolete("Use AddWebObjectActivator instead or use .AddSystemWebDependencyInjection() to configure all containers in your application.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static HttpApplicationHostBuilder RegisterWebObjectActivator(this HttpApplicationHostBuilder builder)
        => builder.AddWebObjectActivator();

    public static HttpApplicationHostBuilder AddWebObjectActivator(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDependencyRegistrar, WebObjectRegistrar>());
        builder.Services.TryAddSingleton<IServiceScopeFactoryProxy, ScopedServiceProviderFactory>();
        builder.Services.AddHostedService<DependencyRegistrarActivator>();

        return builder;
    }

    private sealed partial class WebObjectRegistrar : IDependencyRegistrar, IServiceProvider
    {
        private readonly IServiceProvider _services;

        public WebObjectRegistrar(IServiceProvider _services)
        {
            this._services = _services;

        }

        public string Name => nameof(HttpRuntime.WebObjectActivator);

        public bool IsActive => ReferenceEquals(HttpRuntime.WebObjectActivator, this);

        public void Dispose()
        {
            if (IsActive)
            {
                HttpRuntime.WebObjectActivator = null;
            }
        }

        public bool Enable(bool force)
        {
            if (force || !IsActive)
            {
                HttpRuntime.WebObjectActivator = this;
                return true;
            }

            return false;
        }

        object? IServiceProvider.GetService(Type serviceType)
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
            else if (serviceType.IsPublic && serviceType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length > 0)
            {
                return ActivatorUtilities.CreateInstance(_services, serviceType);
            }
            else
            {
                return CreateNonPublicInstance(serviceType);
            }

            // The implementation of dependency injection in System.Web expects to be able to create instances
            // of non-public and unregistered types.
            static object CreateNonPublicInstance(Type serviceType) => Activator.CreateInstance(
                serviceType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null);
        }
    }

    private sealed class ScopedServiceProviderFactory(IServiceProvider services) : IServiceScopeFactoryProxy
    {
        [Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Handled by caller")]
        public IServiceScopeProxy CreateScope() => new ScopedServiceProvider(services.CreateScope());

        private sealed class ScopedServiceProvider(IServiceScope scope) : IServiceScopeProxy
        {
            public IServiceProvider ServiceProvider => scope.ServiceProvider;

            public void Dispose() => scope.Dispose();
        }
    }

    private sealed partial class DependencyRegistrarActivator(ILogger<DependencyRegistrarActivator> logger, IEnumerable<IDependencyRegistrar> registrars) : IHostedService
    {
        [LoggerMessage(LogLevel.Critical, "{Resolver} was not configured. Most likely it had already been set.")]
        private static partial void LogResolverNotConfigured(ILogger logger, string resolver);

        [LoggerMessage(LogLevel.Trace, "{Resolver} was configured for dependency injection.")]
        private static partial void LogResolverConfigured(ILogger logger, string resolver);

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            foreach (var registrar in registrars)
            {
                registrar.Enable(force: false);

                if (registrar.IsActive)
                {
                    LogResolverConfigured(logger, registrar.Name);
                }
                else
                {
                    LogResolverNotConfigured(logger, registrar.Name);
                }
            }
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

