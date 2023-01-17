// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal abstract class HttpApplicationEventsMiddleware
{
    private readonly RequestDelegate _next;

    public HttpApplicationEventsMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContextCore context)
    {
        if (context.Features.Get<IHttpResponseAdapterFeature>() is { IsEnded: true })
        {
            return Task.CompletedTask;
        }

        return context.Features.Get<IHttpApplicationEventsFeature>() is { } events
            ? InvokeEventsAsync(_next, context, events)
            : _next(context);
    }

    protected abstract Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events);
}
