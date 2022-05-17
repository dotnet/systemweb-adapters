// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class RemoteAppSessionStateExtensions
{
    public static ISystemWebAdapterBuilder AddRemoteAppSession(this ISystemWebAdapterBuilder builder, Action<RemoteAppSessionStateOptions> configureRemote, Action<SessionSerializerOptions> configureSerializer)
    {
        var options = new RemoteAppSessionStateOptions();
        configureRemote(options);

        var serializerOptions = new SessionSerializerOptions();
        configureSerializer(serializerOptions);
        var serializer = new JsonSessionSerializer(serializerOptions.KnownKeys);

        builder.Modules.Add(new RemoteSessionModule(options, serializer));
        return builder;
    }
}
