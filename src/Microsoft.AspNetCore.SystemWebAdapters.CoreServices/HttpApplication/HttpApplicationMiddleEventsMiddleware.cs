// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpApplicationMiddleEventsMiddleware : HttpApplicationEventsMiddleware
{
    public HttpApplicationMiddleEventsMiddleware(RequestDelegate next)
        : base(next)
    {
    }

    protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events)
    {
        await events.RaiseResolveRequestCacheAsync(context.RequestAborted);
        await events.RaisePostResolveRequestCacheAsync(context.RequestAborted);
        await events.RaiseMapRequestHandlerAsync(context.RequestAborted);
        await events.RaisePostMapRequestHandlerAsync(context.RequestAborted);

        await next(context);

        await events.RaiseUpdateRequestCacheAsync(context.RequestAborted);
        await events.RaisePostUpdateRequestCacheAsync(context.RequestAborted);
        await events.RaiseLogRequestAsync(context.RequestAborted);
        await events.RaisePostLogRequestAsync(context.RequestAborted);
    }
}
