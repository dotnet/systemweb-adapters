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

    /// <summary>
    /// Gets or sets the remote service options used to connect with the remote authentication service.
    /// </summary>
    public RemoteServiceOptions RemoteServiceOptions { get; set; } = new RemoteServiceOptions();

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// Gets or sets a list of request headers that should be forwarded to the remote app for authentication purposes. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public IList<string> HeadersToForward { get; set; } = new List<string>(DefaultHeadersToForward);

    /// <summary>
    /// Gets or sets a list of names of cookies that should be forwarded to the remote app for authentication purposes. If no cookies
    /// are specified, all cookies will be forwarded.
    /// </summary>
    public IList<string> CookiesToForward { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a list of response headers that may need propagated back from authenticate responses. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public IList<string> ResponseHeadersToForward { get; set; } = new List<string>(DefaultResponseHeadersToForward);

    /// <summary>
    /// Gets or sets the endpoint on the remote app that provides remote authentication
    /// services. Requests to authenticate are sent to this endpoint.
    /// </summary>
    [Required]
#endif
    public string AuthenticationEndpointPath { get; set; } = "/systemweb-adapters/authenticate";

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);
#endif
}
