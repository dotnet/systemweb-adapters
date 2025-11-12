// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal sealed class ResponseFeaturesMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContextCore context)
    {
        var responseBodyFeature = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();

        using var adapterFeature = new HttpResponseAdapterFeature(responseBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(adapterFeature);
        context.Features.Set<IHttpResponseBufferingFeature>(adapterFeature);
        context.Features.Set<IHttpResponseEndFeature>(adapterFeature);
        context.Features.Set<IHttpResponseContentFeature>(adapterFeature);

        try
        {
            await next(context);
        }
        finally
        {
            // The buffering feature may be removed if the response has ended i.e in usage with YARP
            if (context.Features.Get<IHttpResponseBufferingFeature>() is { } buffer)
            {
                await buffer.FlushAsync();
            }

            context.Features.Set<IHttpResponseBodyFeature>(responseBodyFeature);
            context.Features.Set<IHttpResponseBufferingFeature>(null);
            context.Features.Set<IHttpResponseEndFeature>(null);
            context.Features.Set<IHttpResponseContentFeature>(null);
        }
    }
}
