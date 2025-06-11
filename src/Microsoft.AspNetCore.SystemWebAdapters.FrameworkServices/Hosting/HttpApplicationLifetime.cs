// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
internal sealed class HttpApplicationLifetime : IHostLifetime, IRegisteredObject, IDisposable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<HttpApplicationLifetime> _logger;
    private readonly IHostEnvironment _environment;

    public HttpApplicationLifetime(
        IHostEnvironment environment,
        IHostApplicationLifetime applicationLifetime,
        ILogger<HttpApplicationLifetime> logger
        )
    {
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _environment = environment;

        HostingEnvironment.RegisterObject(this);
    }

    void IRegisteredObject.Stop(bool immediate)
    {
        HostingEnvironment.UnregisterObject(this);
        HostingEnvironment.QueueBackgroundWorkItem(_ => _applicationLifetime.StopApplication());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        HostingEnvironment.UnregisterObject(this);
        _logger.LogInformation("Shutting down application for {Reason}", HostingEnvironment.ShutdownReason);

        return Task.CompletedTask;
    }

    Task IHostLifetime.WaitForStartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogInformation("Application started. Hosting environment: {EnvName}; Content root path: {ContentRoot}",
                _environment.EnvironmentName, _environment.ContentRootPath);
        });
        _applicationLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogInformation("Application is shutting down...");
        });
        _applicationLifetime.ApplicationStopped.Register(() =>
        {
            _logger.LogInformation("Application has stopped.");

            HostingEnvironment.InitiateShutdown();
        });

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        HostingEnvironment.UnregisterObject(this);
    }
}


