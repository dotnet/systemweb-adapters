// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAppAuthenticationModule : IHttpModule
{
    private readonly RemoteAppAuthenticationOptions _authOptions;
    private readonly RemoteAppOptions _remoteAppOptions;

    public RemoteAppAuthenticationModule(IOptions<RemoteAppAuthenticationOptions> authOptions, IOptions<RemoteAppOptions> remoteAppOptions)
    {
        _authOptions = authOptions?.Value ?? throw new ArgumentNullException(nameof(authOptions));
        _remoteAppOptions = remoteAppOptions?.Value ?? throw new ArgumentNullException(nameof(remoteAppOptions));
    }

    public void Init(HttpApplication context)
    {
        var handler = new RemoteAppAuthenticationHttpHandler();

        context.PostMapRequestHandler += MapRemoteAuthenticationHandler;

        void MapRemoteAuthenticationHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, _authOptions.AuthenticationEndpointPath, StringComparison.OrdinalIgnoreCase)
                && context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(_remoteAppOptions.ApiKey, context.Request.Headers.Get(_remoteAppOptions.ApiKeyHeader), StringComparison.Ordinal))
                {
                    // Using 407 here (proxy authentication required) to differentiate from the scenario of
                    // a valid API key but no authenticated user.
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
