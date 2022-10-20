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
    private Uri _remoteAppUrl = null!;

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    [Required]
    public string ApiKeyHeader { get; set; } = RemoteConstants.ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
    [Required, ApiKey]
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteAppUrl
    {
        get => _remoteAppUrl;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(RemoteAppUrl));
            }

            // Path must end in '/' so that it will combine correctly with subpaths
            if (!value.AbsolutePath.EndsWith("/"))
            {
                var builder = new UriBuilder(value);
                builder.Path += "/";
                value = builder.Uri;
            }

            _remoteAppUrl = value;
        }
    }

    /// <summary>
    /// Gets or sets an <see cref="HttpMessageHandler"/> to use for making requests to the remote app. Used if BackchannelClient is null.
    /// </summary>
    public HttpMessageHandler? BackchannelHandler { get; set; }

    /// <summary>
    /// Gets or sets an <see cref="HttpClient"/> to use for making requests to the remote app.
    /// </summary>
    public HttpClient BackchannelClient { get; set; }
        // Set to default, because we'll ensure this is populated with post-configuration
        = default!;
}
