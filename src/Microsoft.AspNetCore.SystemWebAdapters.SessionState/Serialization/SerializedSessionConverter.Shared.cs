// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal class SerializedSessionConverter : JsonConverter<SessionValues>
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
                ((values ??= new()).UnknownKeys ??= new()).Add(key);
            }

            if (!reader.Read())
            {
                throw new InvalidOperationException();
            }

            if (type is null)
            {
                // Even if we don't know the type, we need to move the reader forward to the end of the content for this key so that subsequent items can be read.
                reader.Skip();
            }
            else if (JsonSerializer.Deserialize(ref reader, type, options) is { } result)
            {
                (values ??= new()).Add(key, result);
            }
        }

        return values;
    }

    public override void Write(Utf8JsonWriter writer, SessionValues session, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var key in session.Keys)
        {
            if (session[key] is { } value)
            {
                if (_map.TryGetValue(key, out var type))
                {
                    writer.WritePropertyName(key);
                    JsonSerializer.Serialize(writer, value, type, options);
                }
                else
                {
                    (session.UnknownKeys ??= new()).Add(key);
                }
            }
        }

        writer.WriteEndObject();
    }
}
