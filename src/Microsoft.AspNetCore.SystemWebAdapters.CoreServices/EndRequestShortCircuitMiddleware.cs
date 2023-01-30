// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A middleware that will short circuit things if CompleteRequest/EndRequest has been called 
/// </summary>
internal sealed class EndRequestShortCircuitMiddleware
{
    private readonly RequestDelegate _next;

    public EndRequestShortCircuitMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Features.Get<IHttpResponseAdapterFeature>() is { IsEnded: true })
        {
            return Task.CompletedTask;
        }

        return _next(context);
    }
}
