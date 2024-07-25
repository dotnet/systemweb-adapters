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
    public interface IRemoteAppAuthenticationService
    {
        /// <summary>
        /// Attempts to authenticate a user who made a given HTTP request by forwarding portions of the request to a remote service.
        /// </summary>
        /// <param name="scheme">The scheme being used for authentication.</param>
        /// <param name="request">The request that is to be authenticated.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        Task<RemoteAppAuthenticationResult> AuthenticateAsync(AuthenticationScheme scheme, HttpRequest request, CancellationToken cancellationToken);
    }
}
