// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class RegisterHttpApplicationPreSendEventsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContextCore context)
    {
        var previous = context.Features.GetRequired<IHttpResponseBodyFeature>();
        var feature = new HttpApplicationPreSendEventsResponseBodyFeature(context, previous);

        context.Features.Set<IHttpResponseBodyFeature>(feature);

        await next(context);

        context.Features.Set<IHttpResponseBodyFeature>(previous);
    }
}
