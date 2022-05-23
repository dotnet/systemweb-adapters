// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
#if NETCOREAPP3_1_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Options for connecting to a remote app.
/// </summary>
public class RemoteServiceOptions
{
    internal const string ApiKeyHeaderName = "X-SystemWebAdapter-RemoteAuthentication-Key";

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    public string ApiKeyHeader { get; set; } = ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
#if NETCOREAPP3_1_OR_GREATER
    [Required]
#endif
    public string ApiKey { get; set; } = null!;

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteAppUrl { get; set; } = null!;
#endif
}
