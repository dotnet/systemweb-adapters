// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class BeginEndEventMiddleware
{
    private readonly RequestDelegate _next;

    public BeginEndEventMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(BufferResponseStreamAttribute.DefaultMemoryThreshold, default);

        await context.Features.GetRequired<IHttpApplicationEventsFeature>().RaiseBeginRequestAsync(context.RequestAborted);
        await _next(context);

        await context.Features.GetRequired<IHttpResponseBufferingFeature>().FlushAsync();
    }
}
