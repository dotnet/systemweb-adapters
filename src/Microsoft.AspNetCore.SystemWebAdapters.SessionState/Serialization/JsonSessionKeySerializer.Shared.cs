// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal class JsonSessionKeySerializer : ISessionKeySerializer
{
    private readonly JsonSessionSerializerOptions _options;

    public JsonSessionKeySerializer(IOptions<JsonSessionSerializerOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public bool TryDeserialize(string key, byte[] bytes, out object? obj)
    {
        if (_options.KnownKeys.TryGetValue(key, out var type))
        {
            obj = JsonSerializer.Deserialize(bytes, type);
            return true;
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

        bytes = Array.Empty<byte>();
        return false;
    }
}

