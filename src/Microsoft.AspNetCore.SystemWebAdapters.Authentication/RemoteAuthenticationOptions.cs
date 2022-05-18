// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
#endif

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAuthenticationOptions
#if NETCOREAPP3_1_OR_GREATER
    : AuthenticationSchemeOptions
#endif
{
#if NETCOREAPP3_1_OR_GREATER
    public static readonly string[] DefaultHeadersToForward = new[]
    {
        "Authorization"
    };

    public static readonly string[] DefaultResponseHeadersToForward = new[]
    {
        "Location",
        "Set-Cookie",
        "WWW-Authenticate"
    };
#endif

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
    public Uri RemoteApp { get; set; } = null!;

    public IList<string> HeadersToForward { get; set; } = new List<string>(DefaultHeadersToForward);

    public IList<string> CookiesToForward { get; set; } = new List<string>();

    public IList<string> ResponseHeadersToForward { get; set; } = new List<string>(DefaultResponseHeadersToForward);

    /// <summary>
    /// Gets or sets the endpoint on the remote app that provides remote authentication
    /// services. Requests to authenticate are sent to this endpoint.
    /// </summary>
    [Required]
#endif
    public string AuthenticationEndpointPath { get; set; } = "/fallback/adapter/authenticate";

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);
#endif
}
