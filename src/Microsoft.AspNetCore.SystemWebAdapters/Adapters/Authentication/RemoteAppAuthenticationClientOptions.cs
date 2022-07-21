// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAppAuthenticationClientOptions : AuthenticationSchemeOptions
{
    private static readonly ImmutableArray<string> DefaultRequestHeadersToForward = ImmutableArray.CreateRange(new[]
    {
        "Authorization",
        "Cookie"
    });

    private static readonly ImmutableArray<string> DefaultResponseHeadersToForward = ImmutableArray.CreateRange(new[]
    {
        "Location",
        "Set-Cookie",
        "WWW-Authenticate"
    });

    /// <summary>
    /// Gets or sets a list of request headers that should be forwarded to the remote app for authentication purposes. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public ICollection<string> RequestHeadersToForward { get; } = new HashSet<string>(DefaultRequestHeadersToForward);

    /// <summary>
    /// Gets or sets a list of response headers that may need propagated back from authenticate responses. If no headers
    /// are specified, all headers will be forwarded.
    /// </summary>
    public ICollection<string> ResponseHeadersToForward { get; } = new HashSet<string>(DefaultResponseHeadersToForward);

    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the endpoint on the remote app that provides remote authentication
    /// services. Requests to authenticate are sent to this endpoint.
    /// </summary>
    [Required]
    public string AuthenticationEndpointPath { get; set; } = AuthenticationConstants.DefaultEndpoint;
}
