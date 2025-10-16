// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    internal HttpApplicationHostBuilder(HostApplicationBuilder other)
    {
        _other = other;
    }

    IDictionary<object, object> IHostApplicationBuilder.Properties => ((IHostApplicationBuilder)_other).Properties;

    public IConfigurationManager Configuration => _other.Configuration;

    public IHostEnvironment Environment => _other.Environment;

    public ILoggingBuilder Logging => _other.Logging;

    public IMetricsBuilder Metrics => _other.Metrics;

    public IServiceCollection Services => _other.Services;

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        _other.ConfigureContainer(factory, configure);
    }

    internal HttpApplicationHost Build() => new(_other.Build());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed in the RunAsync method")]
    internal void BuildAndRunInBackground()
    {
        var host = Build();

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        var tcs = new HostShutdownTaskCompletionSource();

        // We register this before starting the host so that if it stops while we're starting we still get notified
        HostingEnvironment.RegisterObject(tcs);

        lifetime.ApplicationStarted.Register(() =>
        {
            tcs.SetResult(true);
        });

        HostingEnvironment.QueueBackgroundWorkItem(host.RunAsync);

        // We should wait for the application to start before returning - otherwise some things may not be available if
        // the caller tries to use them immediately after calling this method, such as HttpRuntime.WebObjectFactory
        tcs.Task.GetAwaiter().GetResult();

        HostingEnvironment.UnregisterObject(tcs);
    }

    private sealed class HostShutdownTaskCompletionSource : TaskCompletionSource<bool>, IRegisteredObject
    {
        void IRegisteredObject.Stop(bool immediate)
        {
            SetCanceled();
        }
    }
}


