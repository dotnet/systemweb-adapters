// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Security.Claims;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// HTTP handler for serving requests to remote authenticaiton endpoint.
/// </summary>
internal sealed class RemoteAppAuthenticationHttpHandler : IHttpHandler
{
    public bool IsReusable => true;

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
            using var writer = new BinaryWriter(context.Response.OutputStream);
            claimsPrincipal.WriteTo(writer);
        }
        else
        {
            context.Response.StatusCode = 401;
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
