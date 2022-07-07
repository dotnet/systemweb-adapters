// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal readonly struct ForwardedHost
{
    public ForwardedHost(string host, string? proto)
    {
        var idx = host.IndexOf(":", 0, StringComparison.Ordinal);

        if (idx < 0)
        {
            ServerName = host;
            Port = GetDefaultPort(proto);
        }
        else
        {
            ServerName = host.Substring(0, idx);
            Port = host.Substring(idx + 1);
        }
    }

    private static string GetDefaultPort(string? proto)
        => string.Equals("https", proto, StringComparison.OrdinalIgnoreCase) ? "443" : "80";

    public string ServerName { get; }

    public string Port { get; }
}
