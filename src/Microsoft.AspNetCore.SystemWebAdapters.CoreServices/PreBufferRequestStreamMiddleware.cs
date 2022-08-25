// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class PreBufferRequestStreamMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PreBufferRequestStreamAttribute _defaultMetadata = new() { IsDisabled = true };

    public PreBufferRequestStreamMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() ?? _defaultMetadata;
        var existing = context.Features.GetRequired<IHttpRequestFeature>();
        var requestFeature = new HttpRequestAdapterFeature(existing, metadata.BufferThreshold, metadata.BufferLimit);

        if(!metadata.IsDisabled)
        {
            await requestFeature.BufferInputStreamAsync(context.RequestAborted);
        }

        context.Response.RegisterForDispose(requestFeature);
        context.Features.Set<IHttpRequestFeature>(requestFeature);
        context.Features.Set<IHttpRequestAdapterFeature>(requestFeature);

        await _next(context);
    }
}
