// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal abstract class RemoteModule : IHttpModule
{
    private readonly Dictionary<string, Func<HttpContextBase, IHttpHandler?>> _mapping = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptions<RemoteAppServerOptions> _options;

    protected RemoteModule(IOptions<RemoteAppServerOptions> options)
    {
        _options = options;
    }

    protected abstract string Path { get; }

    protected void Register(HttpMethod method, Func<HttpContextBase, IHttpHandler?> handler)
    {
        _mapping.Add(method.ToString(), handler);
    }

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
        context.PostMapRequestHandler += (s, _) =>
        {
            var context = ((HttpApplication)s).Context;

            if (!string.Equals(context.Request.Path, Path, StringComparison.Ordinal))
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
        else if (context.Request.HttpMethod is { } method && _mapping.TryGetValue(method, out var action))
        {
            context.Handler = action(context);
        }
        else
        {
            context.Response.StatusCode = 405;
        }
    }
}
