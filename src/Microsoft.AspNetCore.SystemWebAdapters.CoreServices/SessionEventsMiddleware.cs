// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SessionEventsMiddleware
{
    private readonly RequestDelegate _next;

    public SessionEventsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContextCore context)
    {
        var app = context.Features.GetRequired<IHttpApplicationFeature>();
        var session = context.GetAdapter().Session;

        if (session is { IsNewSession: true })
        {
            await app.RaiseEventAsync(ApplicationEvent.SessionEnd);
        }

        await _next(context);

        if (session is { State.IsAbandoned: true })
        {
            await app.RaiseEventAsync(ApplicationEvent.SessionEnd);
        }
    }
}
