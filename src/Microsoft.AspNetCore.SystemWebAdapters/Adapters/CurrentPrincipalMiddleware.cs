// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class CurrentPrincipalMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentPrincipalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
        => context.User is { } ? SetUserAsync(context) : _next(context);

    private async Task SetUserAsync(HttpContext context)
    {
        var current = Thread.CurrentPrincipal;

        try
        {
            Thread.CurrentPrincipal = context.User;

            await _next(context);
        }
        finally
        {
            Thread.CurrentPrincipal = current;
        }
    }
}
