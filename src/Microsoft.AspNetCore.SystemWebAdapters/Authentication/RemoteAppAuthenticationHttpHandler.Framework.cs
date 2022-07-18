// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// HTTP handler for serving requests to remote authenticaiton endpoint.
/// </summary>
internal sealed class RemoteAppAuthenticationHttpHandler : IHttpHandler
{
    private const string OwinChallengeKey = "security.Challenge";
    private const string OwinEnvironmentKey = "owin.Environment";
    private const string RedirectUriKey = ".redirect";

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
            // Setting 401 signals to other components (such as OWIN auth handlers) that authentication is required.
            // Those components can make updates, as needed, based on the specific auth process being used (such as
            // changing to a 302 status code or adding WWW-Authenticate or Location headers.
            context.Response.StatusCode = 401;

            // Setting the redirect path lets OWIN know that external identity providers should not redirect back to
            // this path (/systemweb-adapters/authenticate) but should redirect back to the URL from the ASP.NET Core
            // app that caused this authenticate request to be made.
            SetOwinRedirectPath(context);
        }

        context.ApplicationInstance.CompleteRequest();
    }

    private static void SetOwinRedirectPath(HttpContext context)
    {
        // Check for OWIN environment items
        if (context.Items[OwinEnvironmentKey] is IDictionary<string, object> owinEnvironment)
        {
            // Get the URI that should be redirected to (current (forwarded) host plus referer path)
            var redirectPath = new Uri(context.Request.Url, context.Request.UrlReferrer);

            // Get the OWIN challenge settings from the environment or, if they don't exist,
            // create new challenge settings,
            if (!owinEnvironment.TryGetValue(OwinChallengeKey, out var owinChallengeSettings))
            {
                // The tuple is an array of authentication types and a dictionary of authentication properties
                owinChallengeSettings = new Tuple<string[], IDictionary<string, string>>(Array.Empty<string>(), new Dictionary<string, string>());
                owinEnvironment[OwinChallengeKey] = owinChallengeSettings;
            }

            // Set the .redirect authentication property to the correct redirect path
            ((Tuple<string[], IDictionary<string, string>>)owinChallengeSettings).Item2[RedirectUriKey] = redirectPath.ToString();
        }
    }
}
