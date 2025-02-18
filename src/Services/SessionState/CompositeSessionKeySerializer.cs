// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal sealed partial class CompositeSessionKeySerializer : ICompositeSessionKeySerializer
{
    private readonly ISessionKeySerializer[] _serializers;
    private readonly IOptions<SessionSerializerOptions> _options;
    private readonly ILogger<CompositeSessionKeySerializer> _logger;

    [LoggerMessage(0, LogLevel.Warning, "Could not serialize session value for key '{Key}'")]
    private partial void LogUnknownSessionKeySerialize(string key);

    [LoggerMessage(1, LogLevel.Warning, "Could not deserialize session value for key '{Key}'")]
    private partial void LogUnknownSessionKeyDeserialize(string key);

    public CompositeSessionKeySerializer(IEnumerable<ISessionKeySerializer> serializers, IOptions<SessionSerializerOptions> options, ILogger<CompositeSessionKeySerializer> logger)
    {
        _serializers = serializers.ToArray();
        _options = options;
        _logger = logger;
    }

    public bool TrySerialize(string key, object? value, out byte[] bytes)
    {
        foreach (var serializer in _serializers)
        {
            if (serializer.TrySerialize(key, value, out bytes))
            {
                return true;
            }
        }

        LogUnknownSessionKeySerialize(key);

        if (_options.Value.ThrowOnUnknownSessionKey)
        {
            throw new UnknownSessionKeyException(key);
        }

        bytes = Array.Empty<byte>();
        return false;
    }

    public bool TryDeserialize(string key, byte[] bytes, out object? obj)
    {
        foreach (var serializer in _serializers)
        {
            if (serializer.TryDeserialize(key, bytes, out obj))
            {
                return true;
            }
        }

        LogUnknownSessionKeyDeserialize(key);

        if (_options.Value.ThrowOnUnknownSessionKey)
        {
            throw new UnknownSessionKeyException(key);
        }

        obj = null;
        return false;
    }
}
