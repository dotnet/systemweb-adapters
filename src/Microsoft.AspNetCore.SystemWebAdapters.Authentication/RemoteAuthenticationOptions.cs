// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP3_1_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif

using System.Collections.Generic;
using System;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAuthenticationOptions
{
    internal const string ApiKeyHeaderName = "X-SystemWebAdapter-RemoteAuthentication-Key";
    internal const string ReadOnlyHeaderName = "X-SystemWebAdapter-RemoteAuthentication-RequiredClaim";

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    public string ApiKeyHeader { get; set; } = ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets the header used to store required claims
    /// </summary>
    public string RequiredClaimHeader { get; set; } = ApiKeyHeaderName;

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

    /// <summary>
    /// Gets or sets the endpoint on the remote app that provides remote authentication
    /// services. Requests to authenticate are sent to this endpoint.
    /// </summary>
    [Required]
#endif
    public string AuthenticationEndpointPath { get; set; } = "/fallback/adapter/authenticate";

    /// <summary>
    /// Gets or sets an enumerable of claims to retrieve for the authenticated
    /// user. Passing an empty enumerable will retrieve all claims.
    /// </summary>
    public IEnumerable<string> RequiredClaimNames { get; set; } = Enumerable.Empty<string>();

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);
#endif
}
