// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class HttpResponseAdapterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpResponseAdapterMiddleware> _logger;

    [LoggerMessage(Level = LogLevel.Trace, Message = "Buffering response stream {BufferLimit} {MemoryThreshold}")]
    private partial void LogBuffering(long? bufferLimit, long memoryThreshold);

    public HttpResponseAdapterMiddleware(RequestDelegate next, ILogger<HttpResponseAdapterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContextCore context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<BufferResponseStreamAttribute>() is { IsDisabled: false } metadata)
        {
            LogBuffering(metadata.BufferLimit, metadata.MemoryThreshold);

            context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(metadata.MemoryThreshold, metadata.BufferLimit);
        }

        return _next(context);
    }
}
