// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// The result of a remote authentication action, potentially including the claims principal
/// of the authenticated user, an HTTP status code or HTTP response headers to return to the caller.
/// </summary>
public class RemoteAppAuthenticationResult
{
    /// <summary>
    /// Create an instance of RemoteAppAuthenticationResult
    /// </summary>
    /// <param name="user">The user returned by the remote authenticate call.</param>
    /// <param name="statusCode">The status code returned from the remote authenticate call.</param>
    public RemoteAppAuthenticationResult(ClaimsPrincipal? user, int statusCode)
    {
        User = user;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the user principal returned in the remote authentication result.
    /// This will be null if remote authenticaiton fails.
    /// </summary>
    public ClaimsPrincipal? User { get; }

    /// <summary>
    /// Gets the status code returned in the remote authentication result.
    /// If a user was successfully retrieved, this status code will be 200.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets a dictionary of auth-related headers that may need propagated back
    /// to the caller if remote authentication fails.
    /// </summary>
    public IHeaderDictionary ResponseHeaders { get; } = new HeaderDictionary();
}
