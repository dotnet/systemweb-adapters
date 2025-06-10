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
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public class HttpApplicationHostOptions
{
    public bool RegisterWebObjectActivator { get; set; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Simple status messages")]
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

        var options = host.Services.GetRequiredService<IOptions<HttpApplicationHostOptions>>();
        _services = HostServices.Create(this, host.Services, options.Value.RegisterWebObjectActivator);

        HostingEnvironment.RegisterObject(_services);

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(static state => ((HttpApplicationHost)state).OnApplicationStarted(), this);
        lifetime.ApplicationStopping.Register(static state => ((HttpApplicationHost)state).OnApplicationStopping(), this);

        _current = this;
    }

    public IServiceProvider Services => _services;

    private void OnApplicationStarted()
    {
        _logger.LogInformation("Application started");
    }

    private void OnApplicationStopping()
    {
        _logger.LogInformation("Application is shutting down...");
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _host.StopAsync(cancellationToken);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Disposed of by IIS lifetime management")]
    void IDisposable.Dispose()
    {
        _host.Dispose();
    }

    private class RegisteredHostServices : HostServices
    {
        public RegisteredHostServices(HttpApplicationHost host, IServiceProvider services) : base(host, services)
        {
            if (HttpRuntime.WebObjectActivator is { })
            {
                throw new InvalidOperationException("HttpRuntime.WebObjectActivator is already configured");
            }

            HttpRuntime.WebObjectActivator = this;
        }

        public override object? GetService(Type serviceType)
        {
            if (base.GetService(serviceType) is { } known)
            {
                return known;
            }

            if (!serviceType.IsAbstract)
            {
                return CreateNonPublicInstance(serviceType);
            }

            return null;
        }

        public override void Stop(bool immediate)
        {
            base.Stop(immediate);
            HttpRuntime.WebObjectActivator = null;
        }

        private object CreateNonPublicInstance(Type serviceType) => Activator.CreateInstance(
            serviceType,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
            null,
            null,
            null);
    }

    private class HostServices(HttpApplicationHost host, IServiceProvider services) : IServiceProvider, IRegisteredObject
    {
        public static HostServices Create(HttpApplicationHost host, IServiceProvider services, bool registerWebJobActivator)
            => registerWebJobActivator ? new RegisteredHostServices(host, services) : new HostServices(host, services);

        public virtual object? GetService(Type serviceType)
        {
            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceType == typeof(IHost) || serviceType == typeof(HttpApplicationHost))
            {
                return host;
            }

            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            return services.GetService(serviceType);
        }

        public virtual void Stop(bool immediate)
        {
            using var cts = new CancellationTokenSource(GracefulShutdownTime);

            if (immediate)
            {
                cts.Cancel();
            }

            host.StopAsync(cts.Token).GetAwaiter().GetResult();
            ((IDisposable)host).Dispose();

            _current = null;
        }
    }
}
