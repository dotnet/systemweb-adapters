// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class JsonSessionSerializer
{
    private readonly ILogger<JsonSessionSerializer> _logger;

    public JsonSessionSerializer(IOptions<JsonSessionSerializerOptions> options, ILogger<JsonSessionSerializer> logger)
    {
        _options = options.Value;
        _jsonOptions = BuildJsonOptions(_options);
        _logger = logger;
    }

    public ISessionState? Deserialize(string? input)
    {
        if (input is null)
        {
            return null;
        }

        var session = JsonSerializer.Deserialize<SerializedSessionState>(input, _jsonOptions);

        CheckUnknownSessionKeys(session, false);

        return session;
    }

    public byte[] Serialize(ISessionState sessionState)
    {
        var session = GetSessionState(sessionState);
        var result = JsonSerializer.SerializeToUtf8Bytes(session, _jsonOptions);

        CheckUnknownSessionKeys(session, true);

        return result;
    }

    private void CheckUnknownSessionKeys(SerializedSessionState? session, bool serialize)
    {
        if (session is { UnknownKeys: { Count: > 0 } unknownKeys })
        {
            foreach (var unknown in unknownKeys)
            {
                LogUnknownKey(unknown, serialize);
            }

            if (_options.ThrowOnUnknownSessionKey)
            {
                throw new UnknownSessionKeyException(unknownKeys);
            }
        }
    }

    partial void LogUnknownKey(string unknown, bool serialize)
    {
        if (serialize)
        {
            _logger.LogWarning("Could not serialize unknown session key '{Key}'", unknown);
        }
        else
        {
            _logger.LogWarning("Could not deserialize unknown session key '{Key}'", unknown);
        }
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
