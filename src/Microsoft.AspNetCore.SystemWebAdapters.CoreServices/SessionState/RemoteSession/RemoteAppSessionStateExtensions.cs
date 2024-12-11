// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterRemoteClientAppBuilder AddSessionClient(this ISystemWebAdapterRemoteClientAppBuilder builder, Action<RemoteAppSessionStateClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddTransient<DoubleConnectionRemoteAppSessionManager>();
        builder.Services.AddTransient<SingleConnectionWriteableRemoteAppSessionStateManager>();
        builder.Services.AddTransient<ISessionManager, RemoteAppSessionDispatcher>();

        builder.Services.AddOptions<RemoteAppSessionStateClientOptions>()
            .Configure(configure ?? (_ => { }))
            .PostConfigure<IOptions<RemoteAppClientOptions>>((options, remote) =>
            {
                // The single connection remote app session client requires https to work so if that's not the case, we'll disable it
                if (!string.Equals(remote.Value.RemoteAppUrl.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSingleConnection = false;
                }
            })
            .ValidateDataAnnotations();

        return builder;
    }
}
