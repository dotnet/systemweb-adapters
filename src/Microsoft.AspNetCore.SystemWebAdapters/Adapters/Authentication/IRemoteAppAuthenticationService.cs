// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication
{
    /// <summary>
    /// A service for authenticating an HTTP request with a remote service.
    /// </summary>
    internal interface IRemoteAppAuthenticationService
    {
        /// <summary>
        /// Initializes the remote authentication service for the given scheme.
        /// </summary>
        /// <param name="scheme">The scheme whose configuration the remote authentication service should use for authenticating requests.</param>
        Task InitializeAsync(AuthenticationScheme scheme);

        /// <summary>
        /// Attempts to authenticate a user who made a given HTTP request by forwarding portions of the request to a remote service.
        /// </summary>
        /// <param name="originalRequest">The request originally made by the user to be authenticated.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        Task<RemoteAppAuthenticationResult> AuthenticateAsync(HttpRequest originalRequest, CancellationToken cancellationToken);
    }
}
