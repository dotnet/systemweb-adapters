// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal abstract class RemoteModule : IHttpModule
{
    private readonly IOptions<RemoteAppServerOptions> _options;

    private Func<HttpContextBase, IHttpHandler?>? _get;
    private Func<HttpContextBase, IHttpHandler?>? _put;

    protected RemoteModule(IOptions<RemoteAppServerOptions> options)
    {
        _options = options;
    }

    protected abstract string Path { get; }

    protected void MapGet(Func<HttpContextBase, IHttpHandler?> handler)
        => _get = handler;

    protected void MapPut(Func<HttpContextBase, IHttpHandler?> handler)
        => _put = handler;

    protected bool HasValidApiKey(HttpContextBase context)
    {
        var apiKey = context.Request.Headers.Get(_options.Value.ApiKeyHeader);

        return string.Equals(_options.Value.ApiKey, apiKey, StringComparison.Ordinal);
    }

    protected virtual bool Authenticate(HttpContextBase context)
    {
        if (HasValidApiKey(context))
        {
            return true;
        }

        context.Response.StatusCode = 401;
        return false;
    }

    void IHttpModule.Dispose()
    {
    }

    void IHttpModule.Init(HttpApplication context)
    {
        var appRelativePath = $"~{Path}";

        context.PostMapRequestHandler += (s, _) =>
        {
            var context = ((HttpApplication)s).Context;

            // Compare against the AppRelativeCurrentExecutionFilePath to account for potential virtual directories
            if (!string.Equals(context.Request.AppRelativeCurrentExecutionFilePath, appRelativePath, StringComparison.Ordinal))
            {
                return;
            }

            context.Handler = null;

            HandleRequest(new HttpContextWrapper(context));

            if (context.Handler is null)
            {
                context.ApplicationInstance.CompleteRequest();
            }
        };
    }

    public void HandleRequest(HttpContextBase context)
    {
        if (!Authenticate(context))
        {
        }
        else if (string.Equals("GET", context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase) && _get is { } get)
        {
            context.Handler = get(context);
        }
        else if (string.Equals("PUT", context.Request.HttpMethod, StringComparison.OrdinalIgnoreCase) && _put is { } put)
        {
            context.Handler = put(context);
        }
        else
        {
            context.Response.StatusCode = 405;
        }
    }
}
