// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class JsonSessionSerializer : ISessionSerializer
{
    private readonly JsonSessionSerializerOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    private static JsonSerializerOptions BuildJsonOptions(JsonSessionSerializerOptions options) => new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        AllowTrailingCommas = true,
        WriteIndented = options.Indented,
        Converters =
        {
            new SerializedSessionConverter(options.KnownKeys),
        }
    };

    public byte[] Serialize(string key, object value)
    {
        if (_options.KnownKeys.TryGetValue(key, out var type))
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, type, _jsonOptions);
        }

        LogUnknownKey(key, serialize: true);

        return _options.ThrowOnUnknownSessionKey ? throw new UnknownSessionKeyException(new[] { key }) : Array.Empty<byte>();
    }

    public object? Deserialize(string key, Memory<byte> bytes)
    {
        if (_options.KnownKeys.TryGetValue(key, out var type))
        {
            if (bytes.IsEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize(bytes.Span, type, _jsonOptions);
        }

        LogUnknownKey(key, serialize: false);

        return _options.ThrowOnUnknownSessionKey ? throw new UnknownSessionKeyException(new[] { key }) : null;
    }

    partial void LogUnknownKey(string unknown, bool serialize);
}
