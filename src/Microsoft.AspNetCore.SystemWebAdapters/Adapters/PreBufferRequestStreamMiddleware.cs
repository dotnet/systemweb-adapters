// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class PreBufferRequestStreamMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PreBufferRequestStreamMiddleware> _logger;

    [LoggerMessage(Level = LogLevel.Trace, Message = "Prebuffering request stream")]
    private partial void LogMessage();

    public PreBufferRequestStreamMiddleware(RequestDelegate next, ILogger<PreBufferRequestStreamMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
        => context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() is { IsDisabled: false } metadata
            ? PreBufferAsync(context, metadata)
            : _next(context);


    private async Task PreBufferAsync(HttpContextCore context, PreBufferRequestStreamAttribute metadata)
    {
        // TODO: Should this enforce MaxRequestBodySize? https://github.com/aspnet/AspLabs/pull/447#discussion_r827314309
        LogMessage();

        context.Request.EnableBuffering(metadata.BufferThreshold, metadata.BufferLimit ?? long.MaxValue);

        await context.Request.Body.DrainAsync(context.RequestAborted);
        context.Request.Body.Position = 0;

        await _next(context);
    }
}
