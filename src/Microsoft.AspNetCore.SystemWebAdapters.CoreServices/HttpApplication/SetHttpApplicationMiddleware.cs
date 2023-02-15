// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class SetHttpApplicationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ObjectPool<HttpApplication> _pool;

    public SetHttpApplicationMiddleware(RequestDelegate next, ObjectPool<HttpApplication> pool)
    {
        _next = next;
        _pool = pool;
    }

    /// <summary>
    /// Initializes the registered HttpApplication to force the Start method to be invoked if present.
    /// </summary>
    public static void InitializeHttpApplication(IServiceProvider services)
    {
        var pool = services.GetRequiredService<ObjectPool<HttpApplication>>();
        pool.Return(pool.Get());
    }

    public async Task InvokeAsync(HttpContextCore context)
    {
        var app = _pool.Get();

        try
        {
            context.Features.Set(app);
            app.Context = context;

            SetRequiredFeatures(context, app);

            context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(BufferResponseStreamAttribute.DefaultMemoryThreshold, default);

            await _next(context);

            await context.Features.GetRequired<IHttpResponseEndFeature>().EndAsync();
        }
        finally
        {
            context.Features.Set<IHttpApplicationFeature>(null);
            _pool.Return(app);
        }
    }

    private static void SetRequiredFeatures(HttpContextCore context, HttpApplication application)
    {
        var setNotification = new RequestHttpApplicationFeature(application, context);

        context.Features.Set<IHttpApplicationFeature>(setNotification);
        context.Features.Set<IHttpResponseEndFeature>(setNotification);
    }
}
