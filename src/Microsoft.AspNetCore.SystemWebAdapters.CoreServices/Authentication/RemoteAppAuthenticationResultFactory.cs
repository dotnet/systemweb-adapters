// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Factory type for generating remote authentication results from HTTP responses
/// from a remote app.
/// </summary>
internal class RemoteAppAuthenticationResultFactory : IAuthenticationResultFactory
{
    private readonly IClaimsSerializer _claimsSerializer;

    public RemoteAppAuthenticationResultFactory(IClaimsSerializer claimsSerializer)
    {
        _claimsSerializer = claimsSerializer ?? throw new ArgumentNullException(nameof(claimsSerializer));
    }

    public async Task<RemoteAppAuthenticationResult> CreateRemoteAppAuthenticationResultAsync(HttpResponseMessage response, RemoteAppAuthenticationClientOptions options)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        RemoteAppAuthenticationResult? ret = null;

        // If the result has a 200 status code, attempt to deserialize the ClaimsPrincipal
        if (response.StatusCode == HttpStatusCode.OK)
        {
            using var responseContent = await response.Content.ReadAsStreamAsync();
            ret = new RemoteAppAuthenticationResult(_claimsSerializer.Deserialize(responseContent), (int)response.StatusCode, response.RequestMessage);
        }

        // If the remote authentication result hasn't yet been created, create it without a claims principal
        if (ret is null)
        {
            ret = new RemoteAppAuthenticationResult(null, (int)response.StatusCode, response.RequestMessage);
        }

        // Copy expected response headers
        foreach (var responseHeader in response.Headers)
        {
            if (!options.ResponseHeadersToForward.Any() || options.ResponseHeadersToForward.Contains(responseHeader.Key))
            {
                ret.ResponseHeaders.Add(responseHeader.Key, responseHeader.Value.ToArray());
            }
        }

        return ret;
    }
}
