// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterRemoteAppBuilder AddRemoteAppServerSession(this ISystemWebAdapterRemoteAppBuilder builder, Action<RemoteAppSessionStateOptions>? configureRemote = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddScoped<IHttpModule, RemoteSessionModule>();
        builder.Services.AddSingleton<ILockedSessionCache, InMemoryLockedSessions>();
        builder.Services.AddSingleton<ISessionSerializer, BinarySessionSerializer>();
        builder.Services.AddOptions<SessionSerializerOptions>()
            // We don't want to throw by default on the .NET Framework side as then the error won't be easily visible in the ASP.NET Core app
            .Configure(options => options.ThrowOnUnknownSessionKey = false);
        var options = builder.Services.AddOptions<RemoteAppSessionStateOptions>();

        if (configureRemote is not null)
        {
            options.Configure(configureRemote);
        }

        return builder;
    }
}
