// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configureRemote, Action<JsonSessionSerializerOptions> configureSerializer)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configureRemote is null)
        {
            throw new ArgumentNullException(nameof(configureRemote));
        }

        if (configureSerializer is null)
        {
            throw new ArgumentNullException(nameof(configureSerializer));
        }

        var options = new RemoteAppSessionStateOptions();
        configureRemote(options);

        // We don't want to throw by default on the .NET Framework side as then the error won't be easily visible in the ASP.NET Core app
        var serializerOptions = new JsonSessionSerializerOptions { ThrowOnUnknownSessionKey = false };
        configureSerializer(serializerOptions);

        var serializer = new JsonSessionSerializer(serializerOptions);

        builder.Modules.Add(new RemoteSessionModule(options, new InMemoryLockedSessions(serializer), serializer));

        return builder;
    }
}
