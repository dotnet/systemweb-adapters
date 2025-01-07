// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class RemoteAppSessionStateExtensions
{
    [LoggerMessage(0, LogLevel.Warning, "The remote app session client is configured to use a single connection, but the remote app URL is not HTTPS. Disabling single connection mode.")]
    private static partial void LogSingleConnectionDisabled(ILogger logger);

    public static ISystemWebAdapterRemoteClientAppBuilder AddSessionClient(this ISystemWebAdapterRemoteClientAppBuilder builder, Action<RemoteAppSessionStateClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddTransient<DoubleConnectionRemoteAppSessionManager>();
        builder.Services.AddTransient<SingleConnectionWriteableRemoteAppSessionStateManager>();
        builder.Services.AddTransient<ISessionManager, RemoteAppSessionDispatcher>();

        builder.Services.AddOptions<RemoteAppSessionStateClientOptions>()
            .Configure(configure ?? (_ => { }))
            .PostConfigure<IOptions<RemoteAppClientOptions>, ILogger<RemoteAppClientOptions>>((options, remote, logger) =>
            {
                // The single connection remote app session client requires https to work so if that's not the case, we'll disable it
                if (options.UseSingleConnection && !string.Equals(remote.Value.RemoteAppUrl.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    LogSingleConnectionDisabled(logger);
                    options.UseSingleConnection = false;
                }
            })
            .ValidateDataAnnotations();

        return builder;
    }
}
