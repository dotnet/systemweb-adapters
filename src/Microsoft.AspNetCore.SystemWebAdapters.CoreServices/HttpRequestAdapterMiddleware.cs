// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class HttpRequestAdapterMiddleware
{
    private readonly RequestDelegate _next;

    public HttpRequestAdapterMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        var feature = RegisterRequestFeatures(context);

        if (context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() is { IsDisabled: false } metadata)
        {
            feature.BufferThreshold = metadata.BufferThreshold;

            if (metadata.BufferLimit is { } limit)
            {
                feature.BufferLimit = limit;
            }

            await feature.BufferInputStreamAsync(context.RequestAborted);
        }

        await _next(context);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
    private static IHttpRequestInputStreamFeature RegisterRequestFeatures(HttpContextCore context)
    {
        var existing = context.Features.GetRequired<IHttpRequestFeature>();

        var inputStreamFeature = new HttpRequestInputStreamFeature(existing);

        context.Response.RegisterForDispose(inputStreamFeature);
        context.Features.Set<IHttpRequestFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(inputStreamFeature);
        context.Features.Set<IRequestBodyPipeFeature>(inputStreamFeature);

        return inputStreamFeature;
    }
}
