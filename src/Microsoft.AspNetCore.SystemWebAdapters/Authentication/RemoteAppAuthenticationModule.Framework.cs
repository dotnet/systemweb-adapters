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
        var handler = new RemoteAppAuthenticationHttpHandler(_options);

        context.PostMapRequestHandler += MapRemoteAuthenticationHandler;

        void MapRemoteAuthenticationHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, _options.AuthenticationEndpointPath, StringComparison.OrdinalIgnoreCase)
                && context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(_options.RemoteServiceOptions.ApiKey, context.Request.Headers.Get(_options.RemoteServiceOptions.ApiKeyHeader), StringComparison.Ordinal))
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
