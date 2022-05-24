// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class JsonSessionSerializer : ISessionSerializer
{
    private readonly IDictionary<string, Type> _map;
    private readonly JsonSerializerOptions _options;

    public JsonSessionSerializer(IDictionary<string, Type> map, bool writeIndented = false)
    {
        _map = map;
        _options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = writeIndented,
            Converters =
            {
                new SerializedSessionConverter(map),
            }
        };
    }

    public byte[] Serialize(string key, object value) => _map.TryGetValue(key, out var type)
        ? JsonSerializer.SerializeToUtf8Bytes(value, type, _options)
        : throw new InvalidOperationException($"Key '{key}' is not registered");

    public object? Deserialize(string key, Memory<byte> bytes)
    {
        if (_map.TryGetValue(key, out var type))
        {
            if (bytes.IsEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize(bytes.Span, type, _options);
        }

        throw new InvalidOperationException($"Key '{key}' is not registered");
    }

    private class SerializedSessionConverter : JsonConverter<SessionValues>
    {
        private readonly IDictionary<string, Type> _map;

        public SerializedSessionConverter(IDictionary<string, Type> map)
        {
            _map = map;
        }

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
        public override SessionValues? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#pragma warning restore CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
        {
            SessionValues? values = null;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType is not JsonTokenType.PropertyName || reader.GetString() is not { } key)
                {
                    throw new InvalidOperationException("Key entry must be a string");
                }

                if (!_map.TryGetValue(key, out var type))
                {
                    throw new InvalidOperationException($"Key '{key}' is not registered");
                }

                if (!reader.Read())
                {
                    throw new InvalidOperationException();
                }

                if (JsonSerializer.Deserialize(ref reader, type, options) is { } result)
                {
                    if (values is null)
                    {
                        values = new();
                    }

                    values.Add(key, result);
                }
            }

            return values;
        }

        public override void Write(Utf8JsonWriter writer, SessionValues session, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var key in session.Keys)
            {
                writer.WritePropertyName(key);

                if (session[key] is { } value)
                {
                    if (!_map.TryGetValue(key, out var type))
                    {
                        throw new InvalidOperationException($"Key '{key}' is not registered");
                    }

                    JsonSerializer.Serialize(writer, value, type, options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndObject();
        }
    }
}
