using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal sealed class CompositeSessionKeySerializer : ICompositeSessionKeySerializer
{
    private readonly ISessionKeySerializer[] _serializers;

    public CompositeSessionKeySerializer(IEnumerable<ISessionKeySerializer> serializers)
    {
        _serializers = serializers.ToArray();
    }

    public bool TrySerialize(string key, object value, out byte[] bytes)
    {
        foreach (var serializer in _serializers)
        {
            if (serializer.TrySerialize(key, value, out bytes))
            {
                return true;
            }
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

        obj = null;
        return false;
    }
}
