// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RequestEndShortCircuitMiddleware
{
    private readonly RequestDelegate _next;

    public RequestEndShortCircuitMiddleware(RequestDelegate next)
        => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Features.Get<IHttpResponseEndFeature>() is { IsEnded: true })
        {
            return Task.CompletedTask;
        }

        return _next(context);
    }
}
