// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Simple status messages")]
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

    public static void RegisterHost(Action<HttpApplicationHostBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = HttpApplicationHostBuilder.Create();
        configure(builder);
        builder.InitializeHost();
    }

    public IServiceProvider Services => _host.Services;

    public Task StartAsync(CancellationToken cancellationToken)
        => _host.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => _host.StopAsync(cancellationToken);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Disposed of by IIS lifetime management")]
    void IDisposable.Dispose()
    {
        _host.Dispose();
        _current = null;
    }
}
