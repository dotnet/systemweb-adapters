// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAppAuthenticationModule : RemoteModule
{
    public RemoteAppAuthenticationModule(IOptions<RemoteAppServerOptions> remoteAppOptions, IOptions<RemoteAppAuthenticationServerOptions> authOptions, IClaimsSerializer claimsSerializer)
        : base(remoteAppOptions)
    {
        if (authOptions is null)
        {
            throw new ArgumentNullException(nameof(authOptions));
        }

        Path = authOptions.Value.AuthenticationEndpointPath;

        var handler = new RemoteAppAuthenticationHttpHandler(claimsSerializer);

        MapGet(context => handler);
    }

    protected override string Path { get; }

    protected override bool Authenticate(HttpContextBase context)
    {
        var migrationAuthenticateHeader = context.Request.Headers.Get(AuthenticationConstants.MigrationAuthenticateRequestHeaderName);

        if (migrationAuthenticateHeader is null)
        {
            // If no migration authentication header is present, then this request was not initiated by the ASP.NET Core app
            // for authentication purposes (though, of course, the request likely did proxy through that app).
            //
            // This most likely indicates that an identity provider is redirecting back to the application after
            // authenticating the user. In that case, the original-url query string will indicate the path
            // that the user should be redirected back to.
            var originalUrlPath = context.Request.QueryString[AuthenticationConstants.OriginalUrlQueryParamName];

            // To redirect, an original URL must be present and it must be a relative path
            if (!string.IsNullOrEmpty(originalUrlPath) && originalUrlPath.StartsWith("/", StringComparison.Ordinal))
            {
                context.Response.StatusCode = 302;

                // Redirect back to the provided relative path.
                context.Response.Headers["Location"] = originalUrlPath;
            }
            else
            {
                // A request without a migration authentication header and without a valid original URL to redirect
                // back to is invalid. Return 400 to indicate that the request was malformed.
                context.Response.StatusCode = 400;
            }

            return false;
        }
        else if (!HasValidApiKey(context))
        {
            // Requests to the authentication endpoint must include a valid API key.
            // Requests without an API key or with an invalid API key are considered malformed.
            context.Response.StatusCode = 400;

            return false;
        }

        return true;
    }
}
