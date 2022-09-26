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
    private readonly IClaimsSerializer _claimsSerializer;

    public RemoteAppAuthenticationHttpHandler(IClaimsSerializer claimsSerializer)
    {
        _claimsSerializer = claimsSerializer ??  throw new ArgumentNullException(nameof(claimsSerializer));
    }

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

            _claimsSerializer.Serialize(claimsPrincipal, context.Response.OutputStream);
        }
        else
        {
            // Setting 401 signals to other components (such as OWIN auth handlers) that authentication is required.
            // Those components can make updates, as needed, based on the specific auth process being used (such as
            // changing to a 302 status code or adding WWW-Authenticate or Location headers.
            context.Response.StatusCode = 401;
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
