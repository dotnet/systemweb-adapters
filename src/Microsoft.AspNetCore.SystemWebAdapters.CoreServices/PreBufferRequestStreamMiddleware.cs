// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class PreBufferRequestStreamMiddleware
{
    private readonly RequestDelegate _next;

    public PreBufferRequestStreamMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        RegisterRequestFeatures(context);

        if (context.GetEndpoint()?.Metadata.GetMetadata<PreBufferRequestStreamAttribute>() is { IsDisabled: false } metadata)
        {
            var inputStreamFeature = context.Features.GetRequired<IHttpRequestInputStreamFeature>();

            inputStreamFeature.BufferThreshold = metadata.BufferThreshold;

            if (metadata.BufferLimit is { } limit)
            {
                inputStreamFeature.BufferLimit = limit;
            }

            await inputStreamFeature.BufferInputStreamAsync(context.RequestAborted);
        }

        await _next(context);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = Constants.DisposeIsRegistered)]
    private static void RegisterRequestFeatures(HttpContextCore context)
    {
        var existing = context.Features.GetRequired<IHttpRequestFeature>();

        var adapterFeature = new HttpRequestInputStreamFeature(existing);

        context.Response.RegisterForDispose(adapterFeature);
        context.Features.Set<IHttpRequestFeature>(adapterFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(adapterFeature);
        context.Features.Set<IRequestBodyPipeFeature>(adapterFeature);
    }
}
