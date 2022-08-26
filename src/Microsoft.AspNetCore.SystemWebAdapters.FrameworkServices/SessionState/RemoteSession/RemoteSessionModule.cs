// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class RemoteSessionModule : IHttpModule
{
    private readonly RemoteAppServerOptions _remoteAppOptions;
    private readonly RemoteAppSessionStateServerOptions _sessionOptions;
    private readonly ReadOnlySessionHandler _readonlyHandler;
    private readonly GetWriteableSessionHandler _writeableHandler;
    private readonly StoreSessionStateHandler _saveHandler;

    public RemoteSessionModule(IOptions<RemoteAppSessionStateServerOptions> sessionOptions, IOptions<RemoteAppServerOptions> remoteAppOptions, ILockedSessionCache cache, ISessionSerializer serializer)
    {
        _sessionOptions = sessionOptions?.Value ?? throw new ArgumentNullException(nameof(sessionOptions));
        _remoteAppOptions = remoteAppOptions?.Value ?? throw new ArgumentNullException(nameof(remoteAppOptions));

        if (string.IsNullOrEmpty(_remoteAppOptions.ApiKey))
        {
            throw new ArgumentOutOfRangeException(nameof(_remoteAppOptions.ApiKey), "API key must not be empty.");
        }

        _readonlyHandler = new ReadOnlySessionHandler(serializer);
        _writeableHandler = new GetWriteableSessionHandler(serializer, cache);
        _saveHandler = new StoreSessionStateHandler(cache, _sessionOptions.CookieName);
    }

    public void Init(HttpApplication context)
    {
        context.PostMapRequestHandler += (s, _) =>
        {
            var context = ((HttpApplication)s).Context;

            // Filter out requests that are not the correct path so we don't create a wrapper for every request
            if (!string.Equals(context.Request.Path, _sessionOptions.SessionEndpointPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            MapRemoteSessionHandler(new HttpContextWrapper(context));

            if (context.Handler is null)
            {
                context.ApplicationInstance.CompleteRequest();
            }
        };
    }

    public void MapRemoteSessionHandler(HttpContextBase context)
    {
        if (!string.Equals(_remoteAppOptions.ApiKey, context.Request.Headers.Get(_remoteAppOptions.ApiKeyHeader), StringComparison.Ordinal))
        {
            context.Response.StatusCode = 401;
        }
        else if (context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            context.Handler = GetIsReadonly(context.Request) ? _readonlyHandler : _writeableHandler;
        }
        else if (context.Request.HttpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            context.Handler = _saveHandler;
        }
        else
        {
            // HTTP methods other than GET (read) or PUT (write) are not accepted
            context.Response.StatusCode = 405; // Method not allowed
        }
    }

    public void Dispose()
    {
    }

    private static bool GetIsReadonly(HttpRequestBase request)
        => bool.TryParse(request.Headers.Get(SessionConstants.ReadOnlyHeaderName), out var result) && result;
}
