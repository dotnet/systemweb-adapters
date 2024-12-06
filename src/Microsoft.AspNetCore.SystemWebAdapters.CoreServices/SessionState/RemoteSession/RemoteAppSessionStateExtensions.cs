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
        builder.Services.AddTransient<RemoteAppSessionDispatcher>();
        builder.Services.AddSingleton<ISessionManager>(ctx =>
        {
            var options = ctx.GetRequiredService<IOptions<RemoteAppSessionStateClientOptions>>();

            return options.Value.UseSingleConnection
                ? ctx.GetRequiredService<RemoteAppSessionDispatcher>()
                : ctx.GetRequiredService<DoubleConnectionRemoteAppSessionManager>();
        });

        builder.Services.AddOptions<RemoteAppSessionStateClientOptions>()
            .Configure(configure ?? (_ => { }))
            .ValidateDataAnnotations();

        return builder;
    }
}
