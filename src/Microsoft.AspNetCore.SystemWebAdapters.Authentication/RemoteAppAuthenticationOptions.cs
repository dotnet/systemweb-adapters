// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using System.Collections.Immutable;
#endif

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAppAuthenticationOptions
#if !NETFRAMEWORK
    : AuthenticationSchemeOptions
#endif
{
#if !NETFRAMEWORK
    public static readonly IEnumerable<string> DefaultRequestHeadersToForward = ImmutableArray.CreateRange(new[]
    {
        "Authorization",
        "Cookie"
    });

    public static readonly IEnumerable<string> DefaultResponseHeadersToForward = ImmutableArray.CreateRange(new[]
    {
        "Location",
        "Set-Cookie",
        "WWW-Authenticate"
    });
#endif

    /// <summary>
    /// Gets or sets the remote service options used to connect with the remote authentication service.
    /// </summary>
    public RemoteServiceOptions RemoteServiceOptions { get; set; } = new RemoteServiceOptions();

#if !NETFRAMEWORK
    /// <summary>
    /// Gets or sets a list of request headers that should be forwarded to the remote app for authentication purposes. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public IList<string> RequestHeadersToForward { get; } = new List<string>(DefaultRequestHeadersToForward);

    /// <summary>
    /// Gets or sets a list of response headers that may need propagated back from authenticate responses. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public IList<string> ResponseHeadersToForward { get; } = new List<string>(DefaultResponseHeadersToForward);

    /// <summary>
    /// Gets or sets the endpoint on the remote app that provides remote authentication
    /// services. Requests to authenticate are sent to this endpoint.
    /// </summary>
    [Required]
#endif
    public string AuthenticationEndpointPath { get; set; } = "/systemweb-adapters/authenticate";

#if !NETFRAMEWORK
    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);
#endif
}
