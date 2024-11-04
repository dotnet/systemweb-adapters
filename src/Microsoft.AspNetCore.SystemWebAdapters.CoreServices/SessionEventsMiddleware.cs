// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

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
        if (await RunEventAsync(context, ApplicationEvent.AcquireRequestState))
        {
            return;
        }

        if (context.AsSystemWeb().Session is { IsNewSession: true })
        {
            if (await RunEventAsync(context, ApplicationEvent.SessionStart))
            {
                return;
            }
        }

        if (await RunEventAsync(context, ApplicationEvent.PostAcquireRequestState))
        {
            return;
        }

        await _next(context);

        if (context.Features.GetRequiredFeature<IHttpResponseEndFeature>().IsEnded)
        {
            return;
        }

        if (await RunEventAsync(context, ApplicationEvent.ReleaseRequestState))
        {
            return;
        }

        if (context.AsSystemWeb().Session is { State.IsAbandoned: true })
        {
            if (await RunEventAsync(context, ApplicationEvent.SessionEnd))
            {
                return;
            }
        }

        await context.Features.GetRequiredFeature<IHttpApplicationFeature>().RaiseEventAsync(ApplicationEvent.PostReleaseRequestState);
    }

    private static async ValueTask<bool> RunEventAsync(HttpContextCore context, ApplicationEvent @event)
    {
        await context.Features.GetRequiredFeature<IHttpApplicationFeature>().RaiseEventAsync(@event);

        return context.Features.GetRequiredFeature<IHttpResponseEndFeature>().IsEnded;
    }
}
