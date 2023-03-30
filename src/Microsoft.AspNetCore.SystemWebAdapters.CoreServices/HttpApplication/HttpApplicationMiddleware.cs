// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpApplicationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ObjectPool<HttpApplication> _pool;

    public HttpApplicationMiddleware(RequestDelegate next, ObjectPool<HttpApplication> pool)
    {
        _next = next;
        _pool = pool;
    }

    public async Task InvokeAsync(HttpContextCore context)
    {
        var app = _pool.Get();

        var endFeature = context.Features.GetRequired<IHttpResponseEndFeature>();
        var setNotification = new RequestHttpApplicationFeature(app, endFeature);

        context.Features.Set<IHttpApplicationFeature>(setNotification);
        context.Features.Set<IHttpResponseEndFeature>(setNotification);

        try
        {
            context.Features.Set(app);
            app.Context = context;

            context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(BufferResponseStreamAttribute.DefaultMemoryThreshold, default);

            try
            {
                await _next(context);
            }
            finally
            {
                await context.Features.GetRequired<IHttpResponseEndFeature>().EndAsync();
            }
        }
        finally
        {
            context.Features.Set<IHttpResponseEndFeature>(endFeature);
            context.Features.Set<IHttpApplicationFeature>(null);

            _pool.Return(app);
        }
    }
}
