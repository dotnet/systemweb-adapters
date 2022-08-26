// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Options for connecting to a remote app.
/// </summary>
public class RemoteAppClientOptions
{
    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    [Required]
    public string ApiKeyHeader { get; set; } = RemoteConstants.ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteAppUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets an <see cref="HttpClient"/> to use for making requests to the remote app.
    /// A new <see cref="HttpClient"/> will be automatically generated if none is provided.
    /// </summary>
    public HttpClient? BackchannelHttpClient { get; set; }
}
