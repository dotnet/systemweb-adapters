// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class HandlerMatchPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 0;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        => endpoints.Any(e => e.Metadata.OfType<HttpHandlerRouteMetadata>().Any());

    public Task ApplyAsync(HttpContextCore httpContext, CandidateSet candidates)
    {
        for (var i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].Endpoint.Metadata.GetMetadata<HttpHandlerRouteMetadata>() is { } metadata)
            {
                if (metadata.IsMatch(httpContext.Request.Path))
                {
                    candidates.SetValidity(i, true);
                }
            }
        }

        return Task.CompletedTask;
    }
}

