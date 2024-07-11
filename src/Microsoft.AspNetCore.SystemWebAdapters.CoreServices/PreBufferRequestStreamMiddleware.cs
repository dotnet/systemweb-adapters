// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class PreBufferRequestStreamMiddleware
{
    private readonly RequestDelegate _next;

    public PreBufferRequestStreamMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() is { IsDisabled: false } metadata)
        {
            var inputStreamFeature = context.Features.GetRequiredFeature<IHttpRequestInputStreamFeature>();

            inputStreamFeature.BufferThreshold = metadata.BufferThreshold;

            if (metadata.BufferLimit is { } limit)
            {
                inputStreamFeature.BufferLimit = limit;
            }

            await inputStreamFeature.BufferInputStreamAsync(context.RequestAborted);
        }

        await _next(context);
    }
}
