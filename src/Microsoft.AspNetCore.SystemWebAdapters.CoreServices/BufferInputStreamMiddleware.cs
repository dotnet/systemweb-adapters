// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class BufferInputStreamMiddleware
{
    private readonly RequestDelegate _next;

    public BufferInputStreamMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() is { IsDisabled: false } metadata)
        {
            var feature = context.Features.GetRequired<IHttpRequestInputStreamFeature>();

            feature.BufferThreshold = metadata.BufferThreshold;

            if (metadata.BufferLimit is { } limit)
            {
                feature.BufferLimit = limit;
            }

            await feature.BufferInputStreamAsync(context.RequestAborted);
        }

        await _next(context);
    }
}
