// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        => context.GetEndpoint()?.Metadata.GetMetadata<BufferResponseStreamAttribute>() is { IsDisabled: false } metadata
            ? BufferResponseStreamAsync(context, metadata)
            : _next(context);

    private async Task BufferResponseStreamAsync(HttpContextCore context, BufferResponseStreamAttribute metadata)
    {
        LogBuffering(metadata.BufferLimit, metadata.MemoryThreshold);

        var responseBodyFeature = context.Features.GetRequired<IHttpResponseBodyFeature>();

        await using var bufferedFeature = new HttpRequestAdapterFeature(responseBodyFeature, metadata);

        context.Features.Set<IHttpResponseBodyFeature>(bufferedFeature);
        context.Features.Set<IHttpRequestAdapterFeature>(bufferedFeature);

        try
        {
            await _next(context);
            await bufferedFeature.FlushBufferedStreamAsync();
        }
        finally
        {
            context.Features.Set(responseBodyFeature);
            context.Features.Set<IHttpRequestAdapterFeature>(null);
        }
    }
}
