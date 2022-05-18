// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAuthenticationResultFactory : IAuthenticationResultFactory<RemoteAuthenticationResult>
{
    private readonly ILogger<RemoteAuthenticationResultFactory> _logger;

    public RemoteAuthenticationResultFactory(ILogger<RemoteAuthenticationResultFactory> logger)
    {
        _logger = logger;
    }

    public async Task<RemoteAuthenticationResult> CreateRemoteAuthenticationResultAsync(HttpResponseMessage response, RemoteAuthenticationOptions options)
    {
        var ret = new RemoteAuthenticationResult();

        // Copy the status code to the RemoteAuthenticationResult
        ret.StatusCode = (int)response.StatusCode;

        // Copy expected response headers
        foreach (var headerName in options.ResponseHeadersToForward.Where(h => response.Headers.Contains(h)))
        {
            ret.ResponseHeaders.Add(headerName, response.Headers.GetValues(headerName));
        }

        // If the result has a 200 status code, attempt to deserialize the ClaimsPrincipal
        if (response.StatusCode == HttpStatusCode.OK)
        {
            try
            {
                using var reader = new BinaryReader(await response.Content.ReadAsStreamAsync());
                ret.User = new ClaimsPrincipal(reader);
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "Failed to deserialize remote authentication response as a claims principal despite success status code");
            }
        }

        // TODO : Should we return response content in cases where deserializing the claims principal fails?
        return ret;
    }
}
