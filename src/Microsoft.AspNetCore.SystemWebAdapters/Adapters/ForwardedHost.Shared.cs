// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NETSTANDARD

#pragma warning disable CA2234 // Pass System.Uri objects instead of strings

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal readonly struct ForwardedHost
{
    public ForwardedHost(string host, string? proto)
    {
        var hostString = HostString.FromUriComponent(host);

        ServerName = hostString.Host;
        Port = hostString.Port is int p ? p : GetDefaultPort(proto);
    }

    private static int GetDefaultPort(string? proto)
        => string.Equals("https", proto, StringComparison.OrdinalIgnoreCase) ? 443 : 80;

    public string ServerName { get; }

    public int Port { get; }
}
#endif
