// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

/// <summary>
/// An interface to register known keys for session objects.
/// </summary>
public class JsonSessionSerializerOptions
{
    public JsonSessionSerializerOptions()
    {
        _keyComparer = StringComparer.Ordinal;
        KnownKeys = new Dictionary<string, Type>(_keyComparer);
    }

    private IEqualityComparer<string> _keyComparer;

    /// <summary>
    /// Gets or or sets the equality comparer for the known session keys.
    /// </summary>
    public IEqualityComparer<string> KeyComparer
    {
        get => _keyComparer;
        set
        {
            _keyComparer = value;
            KnownKeys = new Dictionary<string, Type>(KnownKeys, _keyComparer);
        }
    }

    /// <summary>
    /// Gets the mapping of known session keys to types
    /// </summary>
    public IDictionary<string, Type> KnownKeys { get; private set; }

    /// <summary>
    /// Registers a session key name to be of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    public void RegisterKey<T>(string key) => KnownKeys.Add(key, typeof(T));
}
