// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
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

    public async Task InvokeAsync(HttpContextCore context)
    {
        RegisterResponseFeatures(context);

        if (context.GetEndpoint()?.Metadata.GetMetadata<BufferResponseStreamAttribute>() is { IsDisabled: false } metadata)
        {
            LogBuffering(metadata.BufferLimit, metadata.MemoryThreshold);

            context.Features.GetRequired<IHttpResponseBufferingFeature>().EnableBuffering(metadata.MemoryThreshold, metadata.BufferLimit);
        }

        await _next(context);

        await context.Features.GetRequired<IHttpResponseBufferingFeature>().FlushAsync();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
    private static void RegisterResponseFeatures(HttpContextCore context)
    {
        var responseBodyFeature = context.Features.GetRequired<IHttpResponseBodyFeature>();

        var adapterFeature = new HttpResponseAdapterFeature(responseBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(adapterFeature);
        context.Features.Set<IHttpResponseBufferingFeature>(adapterFeature);
        context.Features.Set<IHttpResponseEndFeature>(adapterFeature);

        context.Response.RegisterForDisposeAsync(adapterFeature);
    }
}
