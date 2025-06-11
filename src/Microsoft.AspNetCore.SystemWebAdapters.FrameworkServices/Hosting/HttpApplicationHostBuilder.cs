// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public sealed class HttpApplicationHostBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _other;

    private HttpApplicationHostBuilder(HostApplicationBuilder other)
    {
        _other = other;
    }

    IDictionary<object, object> IHostApplicationBuilder.Properties => ((IHostApplicationBuilder)_other).Properties;

    public IConfigurationManager Configuration => _other.Configuration;

    public IHostEnvironment Environment => _other.Environment;

    public ILoggingBuilder Logging => _other.Logging;

    public IMetricsBuilder Metrics => _other.Metrics;

    public IServiceCollection Services => _other.Services;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
    internal void Initialize()
    {
        var host = new HttpApplicationHost(_other.Build());

        HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
        {
            try
            {
                await host.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow it to ensure the application fails to start
                var logger = host.Services.GetRequiredService<ILogger<HttpApplicationHost>>();
                logger.LogError(ex, "An error occurred while starting the application.");
                throw;
            }
        });
    }

    public static HttpApplicationHostBuilder Create()
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings()
        {
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

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _other.ConfigureContainer(factory, configure);
    }

    internal static IHost? Host { get; set; }

    private sealed class HttpApplicationLifetime : IHostLifetime
    {
        Task IHostLifetime.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task IHostLifetime.WaitForStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

