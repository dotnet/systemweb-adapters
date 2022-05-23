// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Security.Claims;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// HTTP handler for serving requests to remote authenticaiton endpoint.
/// </summary>
internal sealed class RemoteAuthenticationHttpHandler : IHttpHandler
{
    private readonly RemoteAuthenticationOptions _options;

    public bool IsReusable => true;

    public RemoteAuthenticationHttpHandler(RemoteAuthenticationOptions options)
    {
        if (string.IsNullOrEmpty(options.RemoteServiceOptions.ApiKey))
        {
            throw new ArgumentOutOfRangeException("API key must not be empty.");
        }

        _options = options;
    }

    public void ProcessRequest(HttpContext context)
    {
        if (context is null)
        {
            return;
        }

        // TODO : Do we need to even check the API key for this? The handler only returns information
        //        about the currently authenticated user.
        if (!string.Equals(_options.RemoteServiceOptions.ApiKey, context.Request.Headers.Get(_options.RemoteServiceOptions.ApiKeyHeader), StringComparison.Ordinal))
        {
            // Using 407 here (proxy authentication required) to differentiate from the scenario of
            // a valid API key but no authenticated user.
            context.Response.StatusCode = 407;
        }
        else
        {
            // If a user is logged in (using ASP.NET's usual authenticaiton mechanisms), return that claims principal.
            if (context.User is ClaimsPrincipal claimsPrincipal && context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/octet-stream";
                claimsPrincipal.WriteTo(new BinaryWriter(context.Response.OutputStream));
            }
            else
            {
                context.Response.StatusCode = 401;
            }
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
