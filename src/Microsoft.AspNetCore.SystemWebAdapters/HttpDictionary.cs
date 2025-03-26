// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;

namespace System.Web;

internal class HttpDictionary : NameObjectCollectionBase
{
    internal HttpDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
    {
    }

    internal int Size
    {
        get { return Count; }
    }

    internal Object? GetValue(String key) => BaseGet(key);

    internal void SetValue(String key, Object value) => BaseSet(key, value);

    internal Object? GetValue(int index) => BaseGet(index);

    internal String? GetKey(int index) => BaseGetKey(index);

    internal String?[] GetAllKeys() => BaseGetAllKeys();
}
