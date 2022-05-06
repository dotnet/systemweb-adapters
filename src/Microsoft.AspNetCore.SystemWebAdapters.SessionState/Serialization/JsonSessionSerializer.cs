// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class JsonSessionSerializer
{
    public JsonSessionSerializer(IOptions<SessionSerializerOptions> options)
        : this(options.Value.KnownKeys)
    {
    }

    public ISessionState? Deserialize(string? input)
        => input is null ? null : JsonSerializer.Deserialize<SerializedSessionState>(input, _options);

    public byte[] Serialize(ISessionState sessionState)
    {
        var session = GetSessionState(sessionState);

        return JsonSerializer.SerializeToUtf8Bytes(session, _options);
    }

    private static SerializedSessionState GetSessionState(ISessionState state)
    {
        if (state is SerializedSessionState s)
        {
            return s;
        }

        s = new()
        {
            IsAbandoned = state.IsAbandoned,
            IsNewSession = state.IsNewSession,
            IsReadOnly = state.IsReadOnly,
            SessionID = state.SessionID,
            Timeout = state.Timeout,
        };

        foreach (var key in state.Keys)
        {
            if (state[key] is { } value)
            {
                s.Values.Add(key, value);
            }
        }

        return s;
    }
}
