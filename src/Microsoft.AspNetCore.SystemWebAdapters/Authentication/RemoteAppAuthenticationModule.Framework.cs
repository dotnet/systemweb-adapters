// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAppAuthenticationModule : IHttpModule
{
    private readonly RemoteAppAuthenticationOptions _options;

    public RemoteAppAuthenticationModule(RemoteAppAuthenticationOptions options)
    {
        _options = options;
    }

    public void Init(HttpApplication context)
    {
        var handler = new RemoteAppAuthenticationHttpHandler();

        context.PostMapRequestHandler += MapRemoteAuthenticationHandler;

        void MapRemoteAuthenticationHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, _options.AuthenticationEndpointPath, StringComparison.OrdinalIgnoreCase)
                && context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var apiKey = context.Request.Headers.Get(_options.RemoteServiceOptions.ApiKeyHeader);
                if (apiKey is null)
                {
                    // A missing API key may indicate that instead of being a request from the ASP.NET Core app,
                    // this request represents a redirect back from an external authentication provider (for example,
                    // in OIDC or WS-Fed scenarios). In those scenarios, the external authentication provider may attempt
                    // to redirect back to the URL that initiated the login request.
                    //
                    // In those scenarios, if there is an original-url query parameter present in the request's URL, we
                    // can redirect back to that as the *actual* original URL that the authentication process needs to
                    // redirect back to.
                    var originalUrl = context.Request.QueryString[AuthenticationConstants.OriginalUrlQueryParamName];
                    if (originalUrl is not null)
                    {
                        context.Response.StatusCode = 302;
                        context.Response.Headers["Location"] = originalUrl;
                    }
                    else
                    {
                        // A request without a valid API key and without an original URL to redirect
                        // back to is invalid. Return 400 to indicate that the request was malformed.
                        context.Response.StatusCode = 400;
                    }
                    context.ApplicationInstance.CompleteRequest();
                }
                else if (!string.Equals(_options.RemoteServiceOptions.ApiKey, apiKey, StringComparison.Ordinal))
                {
                    // A non-null but invalid API key means that the ASP.NET Core app did not proxy the request
                    // with proper credentials. Return 407 to indicate the error and to differentiate it from
                    // the scenario of a correct API key but not authenticated user (401).
                    context.Response.StatusCode = 407;
                    context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    context.Handler = handler;
                }
            }
        }
    }

    public void Dispose()
    {
    }
}
