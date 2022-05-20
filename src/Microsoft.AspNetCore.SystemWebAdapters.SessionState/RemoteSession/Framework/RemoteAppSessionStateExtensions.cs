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
        var options = new RemoteAppSessionStateOptions();
        configureRemote(options);

        var serializerOptions = new JsonSessionSerializerOptions();
        configureSerializer(serializerOptions);
        var serializer = new JsonSessionSerializer(serializerOptions);

        builder.Modules.Add(new RemoteSessionModule(options, new InMemoryLockedSessions(serializer), serializer));
        return builder;
    }
}
