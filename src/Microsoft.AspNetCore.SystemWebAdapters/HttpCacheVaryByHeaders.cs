// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace System.Web;

public sealed class HttpCacheVaryByHeaders
{
    private readonly HashSet<string> _headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    internal HttpCacheVaryByHeaders()
    {
    }

    public bool this[string name]
    {
        get => _headers.Contains(name);
        set
        {
            // ASP.NET did not allow setting false here - it just ignored it
            if (!value)
            {
                return;
            }

            _headers.Add(name);
        }
    }

    internal bool IsEmpty => _headers.Count == 0;

    public void SetHeaders(string[] headers)
    {
        _headers.UnionWith(headers);
    }

    public string[] GetHeaders() => _headers.ToArray();

    internal string[] GetHeaders(bool omit)
    {
        if (omit)
        {
            return _headers.Where(h => string.Equals(h, "*", StringComparison.Ordinal)).ToArray();
        }
        else
        {
            return GetHeaders();
        }
    }
}
