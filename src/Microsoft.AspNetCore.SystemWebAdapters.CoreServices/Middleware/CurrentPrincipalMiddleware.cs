// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal sealed class CurrentPrincipalMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContextCore context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<SetThreadCurrentPrincipalAttribute>() is { IsDisabled: false })
        {
            context.Features.GetRequiredFeature<IRequestUserFeature>().EnableStaticAccessors();
        }

        return next(context);
    }
}
