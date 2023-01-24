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

/// <summary>
/// Extension methods that enable optionally skipping endpoints
/// </summary>
public static class SkippableEndpointExtensions
{
    /// <summary>
    /// Registers a <see cref="ISkippableEndpointSelector"/> that will be called when selecting
    /// an endpoint that has been marked as skippable by <see cref="EnableSkipping{TBuilder}(TBuilder)"/>.
    /// </summary>
    /// <typeparam name="T">Type of selector</typeparam>
    /// <param name="services">Service collection to add to.</param>
    public static void AddSkippableEndpoint<T>(this IServiceCollection services)
        where T : class, ISkippableEndpointSelector
    {
        services.AddTransient<ISkippableEndpointSelector, T>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, SkippableEndpointMatcherPolicy>());
    }

    /// <summary>
    /// Enable skippable behavior for supplied endpoints. An implementation of <see cref="ISkippableEndpointSelector"/> must be registered as a service for this to be enabled at runtime.
    /// </summary>
    /// <param name="builder">The endpoint convention builder</param>
    /// <returns>The original convention builder.</returns>
    public static TBuilder EnableSkipping<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithMetadata(SkipMetadata.Instance);
    }

    private sealed class SkipMetadata
    {
        public static SkipMetadata Instance { get; } = new();
    }

    private sealed class SkippableEndpointMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
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
