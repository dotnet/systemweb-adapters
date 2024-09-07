// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RegisterHttpApplicationFeatureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ObjectPool<HttpApplication> _pool;

    public RegisterHttpApplicationFeatureMiddleware(RequestDelegate next, ObjectPool<HttpApplication> pool)
    {
        _next = next;
        _pool = pool;
    }

    public async Task InvokeAsync(HttpContextCore context)
    {
        var endFeature = context.Features.GetRequired<IHttpResponseEndFeature>();
        using var httpApplicationFeature = new HttpApplicationFeature(context, endFeature, _pool);

        context.Features.Set<IHttpApplicationFeature>(httpApplicationFeature);
        context.Features.Set<IHttpResponseEndFeature>(httpApplicationFeature);
        context.Features.Set<IRequestExceptionFeature>(httpApplicationFeature);

        try
        {
            await _next(context);
        }
        finally
        {
            context.Features.Set<IHttpResponseEndFeature>(endFeature);
            context.Features.Set<IHttpApplicationFeature>(null);
            context.Features.Set<IRequestExceptionFeature>(null);
        }
    }
}
