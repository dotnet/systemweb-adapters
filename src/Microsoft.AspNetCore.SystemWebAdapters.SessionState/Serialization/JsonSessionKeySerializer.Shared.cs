// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

#if !NETFRAMEWORK
using Microsoft.Extensions.Options;
#endif

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal class JsonSessionKeySerializer : ISessionKeySerializer
{
    private readonly JsonSessionSerializerOptions _options;

#if NETFRAMEWORK
    public JsonSessionKeySerializer(JsonSessionSerializerOptions options)
    {
        _options = options;
    }
#else
    public JsonSessionKeySerializer(IOptions<JsonSessionSerializerOptions> options)
    {
        _options = options.Value;
    }
#endif

    public bool TryDeserialize(string key, ReadOnlyMemory<byte> bytes, out object? obj)
    {
        if (_options.KnownKeys.TryGetValue(key, out var type))
        {
            obj = JsonSerializer.Deserialize(bytes.Span, type);
            return true;
        }

        if (_options.ThrowOnUnknownSessionKey)
        {
            throw new UnknownSessionKeyException(key);
        }

        obj = default;
        return false;
    }

    public bool TrySerialize(string key, object value, out byte[] bytes)
    {
        if (_options.KnownKeys.TryGetValue(key, out var type))
        {
            bytes = JsonSerializer.SerializeToUtf8Bytes(value, type);
            return true;
        }

        if (_options.ThrowOnUnknownSessionKey)
        {
            throw new UnknownSessionKeyException(key);
        }

        bytes = Array.Empty<byte>();
        return false;
    }
}

