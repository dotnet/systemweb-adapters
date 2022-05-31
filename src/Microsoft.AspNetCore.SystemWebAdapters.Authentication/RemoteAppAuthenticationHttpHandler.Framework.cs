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
internal sealed class RemoteAppAuthenticationHttpHandler : IHttpHandler
{
    private readonly RemoteAppAuthenticationOptions _options;

    public bool IsReusable => true;

    public RemoteAppAuthenticationHttpHandler(RemoteAppAuthenticationOptions options)
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

        // If a user is logged in (using ASP.NET's usual authentication mechanisms), return that claims principal.
        if (context.User.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/octet-stream";
            var claimsPrincipal = context.User as ClaimsPrincipal ?? new ClaimsPrincipal(context.User.Identity);
            claimsPrincipal.WriteTo(new BinaryWriter(context.Response.OutputStream));
        }
        else
        {
            context.Response.StatusCode = 401;
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
