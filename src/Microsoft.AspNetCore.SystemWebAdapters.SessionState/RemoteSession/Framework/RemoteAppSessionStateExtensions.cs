// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configureRemote, ISessionKeySerializer keySerializer)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configureRemote is null)
        {
            throw new ArgumentNullException(nameof(configureRemote));
        }

        if (keySerializer is null)
        {
            throw new ArgumentNullException(nameof(keySerializer));
        }

        // We don't want to throw by default on the .NET Framework side as then the error won't be easily visible in the ASP.NET Core app
        var serializerOptions = new SessionSerializerOptions { ThrowOnUnknownSessionKey = false };
        var serializer = new BinarySessionSerializer(keySerializer, serializerOptions);

        return builder.AddRemoteAppSession(configureRemote, serializer);
    }

    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configureRemote, ISessionSerializer serializer)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configureRemote is null)
        {
            throw new ArgumentNullException(nameof(configureRemote));
        }

        if (serializer is null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }

        var remoteOptions = new RemoteAppSessionStateOptions();
        configureRemote(remoteOptions);

        builder.Modules.Add(new RemoteSessionModule(remoteOptions, new InMemoryLockedSessions(serializer), serializer));

        return builder;
    }
}
