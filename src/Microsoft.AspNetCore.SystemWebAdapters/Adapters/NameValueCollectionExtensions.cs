// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
#endif

namespace System.Web;

public static class NameValueCollectionExtensions
{
    public static IEnumerable<string?> EnumerateKeys(this NameValueCollection collection)
    {
#if NET6_0_OR_GREATER
        if (collection is IKeyEnumerator keys)
        {
            return keys.Keys;
        }
        else if (collection is NoGetByIntNameValueCollection)
        {
            return collection.AllKeys;
        }
        else
#endif
        {
            return collection.OfType<string>();
        }
    }
}
