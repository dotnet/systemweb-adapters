// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAppAuthenticationModule : IHttpModule
{
    private readonly RemoteAppAuthenticationOptions _options;
    private readonly RemoteAppAuthenticationHttpHandler _remoteAppAuthHandler;

    public RemoteAppAuthenticationModule(RemoteAppAuthenticationOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrEmpty(options.AuthenticationEndpointPath))
        {
            throw new ArgumentOutOfRangeException(nameof(options.AuthenticationEndpointPath), "Options must specify remote authentication path.");
        }

        if (string.IsNullOrEmpty(options.RemoteServiceOptions.ApiKey))
        {
            throw new ArgumentOutOfRangeException(nameof(options.RemoteServiceOptions.ApiKey), "Options must specify API key.");
        }

        if (string.IsNullOrEmpty(options.RemoteServiceOptions.ApiKeyHeader))
        {
            throw new ArgumentOutOfRangeException(nameof(options.RemoteServiceOptions.ApiKeyHeader), "Options must specify API key header name.");
        }

        _options = options;
        _remoteAppAuthHandler = new RemoteAppAuthenticationHttpHandler();
    }

    public void Init(HttpApplication context)
    {
        context.PostMapRequestHandler += (s, _) =>
        {
            var context = ((HttpApplication)s).Context;
            if (string.Equals(context.Request.Path, _options.AuthenticationEndpointPath, StringComparison.OrdinalIgnoreCase)
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
        var apiKey = context.Request.Headers.Get(_options.RemoteServiceOptions.ApiKeyHeader);
        if (apiKey is null || !string.Equals(_options.RemoteServiceOptions.ApiKey, apiKey, StringComparison.Ordinal))
        {
            // Requests to the authentication endpoint must include a valid API key.
            // Requests without an API key or with an invalid API key are considered malformed.
            context.Response.StatusCode = 400;

            // Clear any existing handler as this endpoint shouldn't respond with a valid API key
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
