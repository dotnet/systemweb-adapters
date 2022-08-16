// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA2234 // Pass System.Uri objects instead of strings

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal readonly struct ForwardedHost
{
    private readonly int? _port;

    public ForwardedHost(string host, string? proto)
    {
        var hostString = HostString.FromUriComponent(host);

        IsSecure = string.Equals("https", proto, StringComparison.OrdinalIgnoreCase);
        ServerName = hostString.Host;
        _port = hostString.Port;
    }

    private int DefaultPort => IsSecure ? 443 : 80;

    public bool IsSecure { get; }

    public string ServerName { get; }

    public int Port => _port is int port ? port : DefaultPort;
}
