// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Security.Claims;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAuthenticationHttpHandler : IHttpHandler
{
    private readonly RemoteAuthenticationOptions _options;

    public bool IsReusable => true;

    public RemoteAuthenticationHttpHandler(RemoteAuthenticationOptions options)
    {
        if (string.IsNullOrEmpty(options.ApiKey))
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
        if (!string.Equals(_options.ApiKey, context.Request.Headers.Get(_options.ApiKeyHeader), StringComparison.Ordinal))
        {
            // Using 407 here (proxy authentication required) to differentiate from the scenario of
            // a valid API key but no authenticated user.
            context.Response.StatusCode = 407;
        }
        else
        {
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
