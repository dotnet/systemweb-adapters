// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

internal sealed partial class HttpApplicationLifetime : IHostLifetime, IRegisteredObject, IDisposable
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<HttpApplicationLifetime> _logger;
    private readonly IHostEnvironment _environment;

    [LoggerMessage(LogLevel.Information, "Shutting down application for {Reason}")]
    private partial void LogShutdown(ApplicationShutdownReason reason);

    [LoggerMessage(LogLevel.Information, "Application started. Hosting environment: {EnvName}; Content root path: {ContentRoot}")]
    private partial void LogStarted(string envName, string contentRoot);

    [LoggerMessage(LogLevel.Information, "Application is shutting down...")]
    private partial void LogStopping();

    [LoggerMessage(LogLevel.Information, "Application has stopped.")]
    private partial void LogStopped();

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
        LogShutdown(HostingEnvironment.ShutdownReason);

        return Task.CompletedTask;
    }

    Task IHostLifetime.WaitForStartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            LogStarted(_environment.EnvironmentName, _environment.ContentRootPath);
        });
        _applicationLifetime.ApplicationStopping.Register(() =>
        {
            LogStopping();
        });
        _applicationLifetime.ApplicationStopped.Register(() =>
        {
            LogStopped();

            HostingEnvironment.InitiateShutdown();
        });

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        HostingEnvironment.UnregisterObject(this);
    }
}


