// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public sealed class HttpApplicationHost : IHost
{
    private static readonly TimeSpan GracefulShutdownTime = TimeSpan.FromSeconds(5);

    private static HttpApplicationHost? _current;

    private readonly IHost _host;
    private readonly ILogger<HttpApplicationHost> _logger;
    private readonly HostServices _services;

    internal static HttpApplicationHost Current => _current ?? throw new InvalidOperationException("Host is not initialized");

    internal HttpApplicationHost(IHost host)
    {
        if (_current is { })
        {
            throw new InvalidOperationException("HttpApplicationHost has already been initialized");
        }

        _host = host;
        _logger = host.Services.GetRequiredService<ILogger<HttpApplicationHost>>(); 
        _services = new HostServices(host);

        HostingEnvironment.RegisterObject(_services);

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(static state => ((HttpApplicationHost)state).OnApplicationStarted(), this);
        lifetime.ApplicationStopping.Register(static state => ((HttpApplicationHost)state).OnApplicationStopping(), this);

        _current = this;
    }

    public IServiceProvider Services => _services;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
    private void OnApplicationStarted()
    {
        _logger.LogInformation("Application started");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
    private void OnApplicationStopping()
    {
        _logger.LogInformation("Application is shutting down...");
    }

    private void StopApplication()
    {
        HostingEnvironment.UnregisterObject(_services);
        HostingEnvironment.InitiateShutdown();
    }

    Task IHost.StartAsync(CancellationToken cancellationToken)
        => _host.StartAsync(cancellationToken);

    Task IHost.StopAsync(CancellationToken cancellationToken)
        => _host.StopAsync(cancellationToken);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Disposed of by IIS lifetime management")]
    void IDisposable.Dispose()
    {
        _host.Dispose();
    }

    private sealed class HostServices(IHost host) : IServiceProvider, IRegisteredObject
    {
        private readonly IServiceProvider _services = host.Services;

        public object? GetService(Type serviceType)
        {
            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceType == typeof(IHost) || serviceType == typeof(ServiceProvider) || serviceType == typeof(HttpApplicationHost))
            {
                return this;
            }

            return _services.GetService(serviceType);
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            using var cts = new CancellationTokenSource(GracefulShutdownTime);

            if (immediate)
            {
                cts.Cancel();
            }

            host.StopAsync(cts.Token).GetAwaiter().GetResult();
            host.Dispose();

            _current = null;
        }
    }

}
