// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class JsonSessionKeySerializer : ISessionKeySerializer
{
    private readonly IOptions<JsonSessionSerializerOptions> _options;
    private readonly ILogger<JsonSessionKeySerializer> _logger;

    public JsonSessionKeySerializer(IOptions<JsonSessionSerializerOptions> options, ILogger<JsonSessionKeySerializer> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [LoggerMessage(0, LogLevel.Error, "Unexpected JSON serialize/deserialization error for '{Key}' expected type '{Type}'")]
    private partial void LogException(Exception e, string key, string type);


    [LoggerMessage(1, LogLevel.Warning, "Session key '{Key}' is registered as {RegisteredType} but was actually {FoundType}")]
    private partial void UnexpectedType(string key, Type registeredType, Type foundType);

    public bool TryDeserialize(string key, byte[] bytes, out object? obj)
    {
        if (_options.Value.KnownKeys.TryGetValue(key, out var type))
        {
            try
            {
                obj = JsonSerializer.Deserialize(bytes, type);
                return true;
            }
            catch (JsonException e)
            {
                LogException(e, key, type.Name);
            }
        }

        obj = default;
        return false;
    }

    public bool TrySerialize(string key, object? value, out byte[] bytes)
    {
        if (_options.Value.KnownKeys.TryGetValue(key, out var type))
        {
            if (value is null)
            {
                if (!type.IsValueType || IsNullable(type))
                {
                    // Create a new one instead of caching since technically the array values could be overwritten
                    bytes = "null"u8.ToArray();
                    return true;
                }
            }
            else if (type == value.GetType() || IsNullableType(type, value.GetType()))
            {
                try
                {
                    bytes = JsonSerializer.SerializeToUtf8Bytes(value, type);
                    return true;
                }
                catch (JsonException e)
                {
                    LogException(e, key, type.Name);
                }
            }
            else
            {
                UnexpectedType(key, type, value.GetType());
            }
        }

        bytes = Array.Empty<byte>();
        return false;
    }

    private static bool IsNullable(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    private static bool IsNullableType(Type type, Type nullableArg)
        => IsNullable(type) && nullableArg == type.GenericTypeArguments[0];
}

