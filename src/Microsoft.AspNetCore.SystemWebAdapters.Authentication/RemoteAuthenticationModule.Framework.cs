// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAuthenticationModule : IHttpModule
{
    private readonly RemoteAuthenticationOptions _options;

    public RemoteAuthenticationModule(RemoteAuthenticationOptions options)
    {
        _options = options;
    }

    public void Init(HttpApplication context)
    {
        var handler = new RemoteAuthenticationHandler(_options);

        context.PostMapRequestHandler += MapRemoteAuthenticationHandler;

        void MapRemoteAuthenticationHandler(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (string.Equals(context.Request.Path, _options.AuthenticationEndpointPath)
                && context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                context.Handler = handler;
            }
        }
    }

    public void Dispose()
    {
    }
}
