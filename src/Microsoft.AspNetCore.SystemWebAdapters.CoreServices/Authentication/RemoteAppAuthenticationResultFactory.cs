// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Factory type for generating remote authentication results from HTTP responses
/// from a remote app.
/// </summary>
internal class RemoteAppAuthenticationResultFactory : IAuthenticationResultFactory
{
    public async Task<RemoteAppAuthenticationResult> CreateRemoteAppAuthenticationResultAsync(HttpResponseMessage response, RemoteAppAuthenticationClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(options);

        RemoteAppAuthenticationResult? ret = null;

        // If the result has a 200 status code, attempt to deserialize the ClaimsPrincipal
        if (response.StatusCode == HttpStatusCode.OK)
        {
            using var responseContent = await response.Content.ReadAsStreamAsync();
            using var reader = new BinaryReader(responseContent);
            ret = new RemoteAppAuthenticationResult(new ClaimsPrincipal(reader), (int)response.StatusCode, response.RequestMessage);
        }

        // If the remote authentication result hasn't yet been created, create it without a claims principal
        if (ret is null)
        {
            ret = new RemoteAppAuthenticationResult(null, (int)response.StatusCode, response.RequestMessage);
        }

        // Copy expected response headers
        bool forwardAllResponseHeaders = options.ResponseHeadersToForward.Count == 0;
        foreach (var responseHeader in response.Headers)
        {
            if (forwardAllResponseHeaders || options.ResponseHeadersToForward.Contains(responseHeader.Key))
            {
                ret.ResponseHeaders.AppendList(responseHeader.Key, responseHeader.Value.ToList());
            }
        }

        return ret;
    }
}
