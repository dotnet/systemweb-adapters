// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HttpApplicationMiddleware
{
    private readonly RequestDelegate _next;

    public HttpApplicationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContextCore context)
    {
        context.Features.GetRequiredFeature<IHttpResponseBufferingFeature>().EnableBuffering(BufferResponseStreamAttribute.DefaultMemoryThreshold, default);

        try
        {
            await _next(context);
        }
        finally
        {
            await context.Features.GetRequiredFeature<IHttpResponseEndFeature>().EndAsync();
        }
    }
}
