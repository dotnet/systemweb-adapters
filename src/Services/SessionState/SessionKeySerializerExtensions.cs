// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal static class SessionSerializerExtensions
{
    public static bool TrySerialize(this ISessionKeySerializer[] serializers, string key, object value, out byte[] bytes)
    {
        foreach (var serializer in serializers)
        {
            if (serializer.TrySerialize(key, value, out bytes))
            {
                return true;
            }
        }

        bytes = Array.Empty<byte>();
        return false;
    }

    internal static bool TryDeserialize(this ISessionKeySerializer[] serializers, string key, byte[] bytes, out object? obj)
    {
        foreach (var serializer in serializers)
        {
            if (serializer.TryDeserialize(key, bytes, out obj))
            {
                return true;
            }
        }

        obj = null;
        return false;
    }
}
