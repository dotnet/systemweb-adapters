// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class SkippableEndpointExtensions
{
    public static void AddSkippableEndpoint<T>(this IServiceCollection services)
        where T : class, ISkippableEndpointSelector
    {
        services.AddTransient<ISkippableEndpointSelector, T>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, SkippableEndpointMatcherPolicy>());
    }

    public static IEndpointConventionBuilder EnableSkipping(this IEndpointConventionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Add(builder => builder.Metadata.Add(SkipMetadata.Instance));
        return builder;
    }

    private class SkipMetadata
    {
        public static SkipMetadata Instance { get; } = new();
    }

    private class SkippableEndpointMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly ISkippableEndpointSelector _selector;

        public SkippableEndpointMatcherPolicy(ISkippableEndpointSelector selector)
        {
            _selector = selector;
        }

        public override int Order => 0;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            => endpoints.Any(e => e.Metadata.GetMetadata<SkipMetadata>() is not null);

        public async Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (await _selector.ShouldSkipAsync(httpContext))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].Endpoint.Metadata.GetMetadata<SkipMetadata>() is not null)
                    {
                        candidates.SetValidity(i, false);
                    }
                }
            }
        }
    }
}
