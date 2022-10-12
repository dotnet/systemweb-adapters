// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Interface for creating authentication results from HTTP responses returned
/// by a remote app in remote authentication scenarios.
/// </summary>
internal interface IAuthenticationResultFactory
{
    /// <summary>
    /// Given an HTTP response and remote authentication options, generate a RemoteAppAuthenticationResult
    /// indicating the result of the remote authentication.
    /// </summary>
    /// <param name="response">The HTTP response from the remote authentication service.</param>
    /// <param name="options">Configuration for remote authentication, including response headers to be propagated to the caller.</param>
    /// <returns>The result of authenticaiton, including a user identity and/or an HTTP status code and response headers.</returns>
    Task<RemoteAppAuthenticationResult> CreateRemoteAppAuthenticationResultAsync(HttpResponseMessage response, RemoteAppAuthenticationClientOptions options);
}
