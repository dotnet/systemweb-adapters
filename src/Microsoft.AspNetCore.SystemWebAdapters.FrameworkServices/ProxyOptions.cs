// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class ProxyOptions
{
    /// <summary>
    /// Gets or sets whether the X-Forwarded-* headers should be used for incoming requests.
    /// </summary>
    public bool UseForwardedHeaders { get; set; }

    /// <summary>
    /// Gets or sets the header name used for the original Host header when the X-Forwarded-Host header is used.
    /// </summary>
    public string OriginalHostHeaderName { get; set; } = "X-Original-Host";

    /// <summary>
    /// Gets or sets the header name used for the original remote IP address when the X-Forwarded-For header is used.
    /// </summary>
    public string OriginalForHeaderName { get; set; } = "X-Original-For";

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the server port.
    /// </summary>
    public int ServerPort { get; set; } = 443;

    /// <summary>
    /// Gets or sets the scheme.
    /// </summary>
    public string Scheme { get; set; } = "https";
}
