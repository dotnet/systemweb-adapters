// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Web.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public sealed class HttpApplicationHost : IHost
{
    private static HttpApplicationHost? _current;

    private readonly IHost _host;

    public static HttpApplicationHost Current => _current ?? throw new InvalidOperationException("Host is not initialized");

    internal HttpApplicationHost(IHost host)
    {
        if (_current is { })
        {
            throw new InvalidOperationException("HttpApplicationHost has already been initialized");
        }

        _host = host;
        _current = this;
    }

    public static HttpApplicationHostBuilder CreateBuilder()
    {
        var config = new ConfigurationManager();

        config.AddConfigurationManager();

        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings()
        {
            Configuration = config,
            ApplicationName = HostingEnvironment.SiteName,
            ContentRootPath = HostingEnvironment.ApplicationPhysicalPath,
            EnvironmentName = HostingEnvironment.IsDevelopmentEnvironment || IsIISExpress() ? Environments.Development : Environments.Production
        });

        builder.Services.AddSingleton<IHostLifetime, HttpApplicationLifetime>();

        return new(builder);

        static bool IsIISExpress()
        {
            using var currentProcess = Process.GetCurrentProcess();

            return string.Equals(currentProcess.ProcessName, "iisexpress", StringComparison.Ordinal);
        }
    }

    public static void RegisterHost(Action<HttpApplicationHostBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = CreateBuilder();
        configure(builder);
        builder.InitializeHost();
    }

    public IServiceProvider Services => _host.Services;

    public Task StartAsync(CancellationToken cancellationToken)
        => _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _host.StopAsync(cancellationToken);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Disposed of by IIS lifetime management and this is a sealed class")]
    void IDisposable.Dispose()
    {
        _host.Dispose();
        _current = null;
    }
}
