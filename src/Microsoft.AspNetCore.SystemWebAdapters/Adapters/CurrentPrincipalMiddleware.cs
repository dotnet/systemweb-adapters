// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class CurrentPrincipalMiddleware
{
    [LoggerMessage(0, LogLevel.Warning, "Using Thread.CurrentPrincipal or ClaimsPrincipal.Current has potential issues and should not be used")]
    private partial void LogCurrentPrincipalWarning();

    [LoggerMessage(1, LogLevel.Warning, "Requests should be limited to a single logical thread using ISingleThreadedRequestMetadata when using Thread.CurrentPrincipal or ClaimsPrincipal.Current")]
    private partial void LogShouldBeSingleThreaded();

    private readonly RequestDelegate _next;
    private readonly ILogger<CurrentPrincipalMiddleware> _logger;

    public CurrentPrincipalMiddleware(RequestDelegate next, ILogger<CurrentPrincipalMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<ISetThreadCurrentPrincipal>() is { IsEnabled: true } ? SetUserAsync(context) : _next(context);

    private async Task SetUserAsync(HttpContext context)
    {
        LogCurrentPrincipalWarning();

        if (context.GetEndpoint()?.Metadata.GetMetadata<ISingleThreadedRequestMetadata>() is not { IsEnabled: true })
        {
            LogShouldBeSingleThreaded();
        }

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
