// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class BufferResponseStreamMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BufferResponseStreamMiddleware> _logger;

    [LoggerMessage(Level = LogLevel.Trace, Message = "Buffering response stream {BufferLimit} {MemoryThreshold}")]
    private partial void LogBuffering(long? bufferLimit, long memoryThreshold);

    public BufferResponseStreamMiddleware(RequestDelegate next, ILogger<BufferResponseStreamMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<BufferResponseStreamAttribute>() is { IsDisabled: false } metadata)
        {
            LogBuffering(metadata.BufferLimit, metadata.MemoryThreshold);

            context.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().EnableBuffering(metadata.MemoryThreshold, metadata.BufferLimit);
        }

        return _next(context);
    }
}
