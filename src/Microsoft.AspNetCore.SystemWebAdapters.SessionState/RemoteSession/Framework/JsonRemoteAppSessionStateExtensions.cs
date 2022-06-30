// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class JsonRemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddJsonRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configureRemote, Action<JsonSessionSerializerOptions> configureSerializer)
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

        var serializerOptions = new JsonSessionSerializerOptions();
        configureSerializer(serializerOptions);

        var keySerializer = new JsonSessionKeySerializer(serializerOptions);

        return builder.AddRemoteAppSession(configureRemote, keySerializer);
    }
}
