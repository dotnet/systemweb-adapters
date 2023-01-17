// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpApplicationEventsHandlerMiddleware : HttpApplicationEventsMiddleware
{
    public HttpApplicationEventsHandlerMiddleware(RequestDelegate next)
        : base(next)
    {
    }

    protected override async Task InvokeEventsAsync(RequestDelegate next, HttpContext context, IHttpApplicationEventsFeature events)
    {
        await events.RaisePreRequestHandlerExecuteAsync(context.RequestAborted);
        await next(context);
        await events.RaisePostRequestHandlerExecuteAsync(context.RequestAborted);
    }
}
