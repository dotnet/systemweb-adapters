// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication.ResultProcessors;

/// <summary>
/// Processes remote authentication results to fix-up 'ReturnUrl' parameters
/// given in redirect responses so that they will return the user to the
/// ASP.NET Core app rather than URLs with the original ASP.NET app's host.
/// </summary>
internal class RedirectUrlProcessor : IRemoteAppAuthenticationResultProcessor
{
    private const string LocationHeaderName = "Location";
    private const string ReturnUrlQueryStringName = "ReturnUrl";

    /// <summary>
    /// Updates response headers in a remote authentication result so that any
    /// 'Location' headers will redirect to the correct (ASP.NET Core) host.
    /// </summary>
    /// <param name="result">The remote app authentication result to be updated so that redirects point to the right locations.</param>
    /// <param name="context">The HTTP context containing the original request that prompted authentication.</param>
    public Task ProcessAsync(RemoteAppAuthenticationResult result, HttpContext context)
    {
        if (result.ResponseHeaders.TryGetValue(LocationHeaderName, out var locationHeaders))
        {
            // Look for any Location headers
            var processedRedirectLocations = new List<string>();
            for (var i = 0; i < locationHeaders.Count; i++)
            {
                var headerValue = locationHeaders[i];
                if (Uri.TryCreate(headerValue, UriKind.RelativeOrAbsolute, out var redirectLocation))
                {
                    // Update the host for any Location headers so that if they point to a URL
                    // in the old app they instead use the new app's host.
                    redirectLocation = UpdateHost(redirectLocation, result.AuthenticationRequest);
                    processedRedirectLocations.Add(redirectLocation.ToString());
                }
            }
            result.ResponseHeaders.Remove(LocationHeaderName);
            result.ResponseHeaders.Add(LocationHeaderName, processedRedirectLocations.ToArray());
        }

        return Task.CompletedTask;
    }

    private static Uri UpdateHost(Uri redirectLocation, HttpRequestMessage? authenticationRequest)
    {
        // No need to update the host if the redirect location is relative (so that it has no host)
        // or if the authentication result does not include the original request (so that we can't
        // compare the host to any host-forward headers in that request).
        if (!redirectLocation.IsAbsoluteUri || authenticationRequest is null)
        {
            return redirectLocation;
        }

        var redirectBuilder = new UriBuilder(redirectLocation);
        authenticationRequest.Headers.TryGetValues(AuthenticationConstants.ForwardedHostHeaderName, out var forwardedHosts);
        authenticationRequest.Headers.TryGetValues(AuthenticationConstants.ForwardedProtoHeaderName, out var forwardedProtos);

        // Only apply if the authenticate request contains forwarded host or scheme
        var forwardedHostAndPort = forwardedHosts?.FirstOrDefault();
        var forwardedProto = forwardedProtos?.FirstOrDefault();

        if (!string.IsNullOrEmpty(forwardedHostAndPort))
        {
            var forwardedHost = new ForwardedHost(forwardedHostAndPort, forwardedProto);

            // If the result's redirect goes to the same host and port as the authentication request went to,
            // replace the host with the forwarded host value.
            if (redirectLocation.Host.Equals(authenticationRequest.RequestUri?.Host, StringComparison.OrdinalIgnoreCase)
                && redirectLocation.Port.Equals(authenticationRequest.RequestUri?.Port))
            {
                redirectBuilder.Host = forwardedHost.ServerName;
                if (int.TryParse(forwardedHost.Port, out var port))
                {
                    redirectBuilder.Port = port;
                }

                // Also replace the scheme if a forwarded scheme was provided
                if (!string.IsNullOrEmpty(forwardedProto))
                {
                    redirectBuilder.Scheme = forwardedProto;
                }
            }
        }

        return redirectBuilder.Uri;
    }

    private static Uri GetAbsoluteUri(Uri redirectLocation, HttpRequest originalRequestPath)
    {
        var baseUri = originalRequestPath.Host.Port.HasValue
            ? new UriBuilder(originalRequestPath.Scheme, originalRequestPath.Host.Host, originalRequestPath.Host.Port.Value).Uri
            : new UriBuilder(originalRequestPath.Scheme, originalRequestPath.Host.Host).Uri;

        return new Uri(baseUri, redirectLocation);
    }
}
