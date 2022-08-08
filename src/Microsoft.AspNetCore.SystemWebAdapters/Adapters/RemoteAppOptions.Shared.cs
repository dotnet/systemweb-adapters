// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
#endif

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Options for connecting to a remote app.
/// </summary>
public class RemoteAppOptions
{
    internal const string ApiKeyHeaderName = "X-SystemWebAdapter-RemoteAppAuthentication-Key";

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
#if NET6_0_OR_GREATER
    [Required]
#endif
    public string ApiKeyHeader { get; set; } = ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
#if NET6_0_OR_GREATER
    [Required]
#endif
    public string ApiKey { get; set; } = null!;

#if NET6_0_OR_GREATER
    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteAppUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets an HttpClient to use for making requests to the remote app.
    /// A new HttpClient will be automatically generated if none is provided.
    /// </summary>
    public HttpClient? BackchannelHttpClient { get; set; }
#endif
}
