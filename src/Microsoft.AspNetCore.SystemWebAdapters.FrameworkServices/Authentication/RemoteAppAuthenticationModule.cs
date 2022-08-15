// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Security.Policy;
using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAppAuthenticationModule : IHttpModule
{
    private readonly RemoteAppOptions _remoteAppOptions;
    private readonly RemoteAppAuthenticationServerOptions _authOptions;
    private readonly RemoteAppAuthenticationHttpHandler _remoteAppAuthHandler;

    public RemoteAppAuthenticationModule(IOptions<RemoteAppOptions> remoteAppOptions, IOptions<RemoteAppAuthenticationServerOptions> authOptions)
    {
        if (authOptions is null)
        {
            throw new ArgumentNullException(nameof(authOptions));
        }

        if (remoteAppOptions is null)
        {
            throw new ArgumentNullException(nameof(remoteAppOptions));
        }

        if (string.IsNullOrEmpty(authOptions.Value.AuthenticationEndpointPath))
        {
            throw new ArgumentOutOfRangeException(nameof(authOptions.Value.AuthenticationEndpointPath), "Options must specify remote authentication path.");
        }

        if (string.IsNullOrEmpty(remoteAppOptions.Value.ApiKey))
        {
            throw new ArgumentOutOfRangeException(nameof(remoteAppOptions.Value.ApiKey), "Options must specify API key.");
        }

        if (string.IsNullOrEmpty(remoteAppOptions.Value.ApiKeyHeader))
        {
            throw new ArgumentOutOfRangeException(nameof(remoteAppOptions.Value.ApiKeyHeader), "Options must specify API key header name.");
        }

        _authOptions = authOptions.Value;
        _remoteAppOptions = remoteAppOptions.Value;
        _remoteAppAuthHandler = new RemoteAppAuthenticationHttpHandler();
    }

    public void Init(HttpApplication context)
    {
        context.PostMapRequestHandler += (s, _) =>
        {
            var context = ((HttpApplication)s).Context;
            if (string.Equals(context.Request.Path, _authOptions.AuthenticationEndpointPath, StringComparison.OrdinalIgnoreCase)
                && context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                MapRemoteAuthenticationHandler(new HttpContextWrapper(context));

                if (context.Handler is null)
                {
                    context.ApplicationInstance.CompleteRequest();
                }
            }
        };
    }

    public void MapRemoteAuthenticationHandler(HttpContextBase context)
    {
        var apiKey = context.Request.Headers.Get(_remoteAppOptions.ApiKeyHeader);
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

            // Clear any existing handler as this request is now completely handled
            context.Handler = null;
        }
        else if (apiKey is null || !string.Equals(_remoteAppOptions.ApiKey, apiKey, StringComparison.Ordinal))
        {
            // Requests to the authentication endpoint must include a valid API key.
            // Requests without an API key or with an invalid API key are considered malformed.
            context.Response.StatusCode = 400;

            // Clear any existing handler as this request is now completely handled
            context.Handler = null;
        }
        else
        {
            context.Handler = _remoteAppAuthHandler;
        }
    }

    public void Dispose()
    {
    }
}
