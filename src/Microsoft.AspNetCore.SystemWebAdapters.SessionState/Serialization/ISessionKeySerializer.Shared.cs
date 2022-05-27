// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface ISessionKeySerializer
{
    /// <summary>
    /// Gets an id for the serializer. This is used to compare payloads to ensure the same serializer is used on both ends.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Serializes an object for a given key.
    /// </summary>
    /// <param name="key">Session key for object.</param>
    /// <param name="value">Object to serialize.</param>,
    /// <param name="bytes">Bytes if sucessful.</param>
    /// <returns><c>true</c> if successful. If key is unknown, <c>false</c> will be returned.</returns>
    bool TrySerialize(string key, object value, out byte[] bytes);

    /// <summary>
    /// Deserializes a session object for a given key.
    /// </summary>
    /// <param name="key">Session key for object.</param>
    /// <param name="bytes">Data to deserialize.</param>
    /// <param name="obj">Deserialized object if successful.</param>
    /// <returns>True if successful. If key is unknown, <c>false</c> will be returned.</returns>
    bool TryDeserialize(string key, ReadOnlyMemory<byte> bytes, out object? obj);
}
