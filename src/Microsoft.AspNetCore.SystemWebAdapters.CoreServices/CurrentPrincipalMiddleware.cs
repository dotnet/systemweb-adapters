// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class CurrentPrincipalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CurrentPrincipalMiddleware> _logger;

    public CurrentPrincipalMiddleware(RequestDelegate next, ILogger<CurrentPrincipalMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<SetThreadCurrentPrincipalAttribute>() is { IsDisabled: false })
        {
            context.GetRequestUser().EnableStaticAccessors();
        }

        return _next(context);
    }
}
