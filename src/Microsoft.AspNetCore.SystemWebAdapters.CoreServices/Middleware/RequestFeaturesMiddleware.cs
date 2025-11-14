// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal sealed class RequestFeaturesMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContextCore context)
    {
        var existing = context.Features.GetRequiredFeature<IHttpRequestFeature>();
        var existingPipe = context.Features.Get<IRequestBodyPipeFeature>();

        using var inputStreamFeature = new HttpRequestInputStreamFeature(existing);

        context.Features.Set<IHttpRequestFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestInputStreamFeature>(inputStreamFeature);
        context.Features.Set<IRequestBodyPipeFeature>(inputStreamFeature);
        context.Features.Set<IHttpRequestPathFeature>(inputStreamFeature);

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set<IHttpRequestFeature>(existing);
            context.Features.Set<IRequestBodyPipeFeature>(existingPipe);
            context.Features.Set<IHttpRequestInputStreamFeature>(null);
            context.Features.Set<IHttpRequestPathFeature>(null);
        }
    }
}
