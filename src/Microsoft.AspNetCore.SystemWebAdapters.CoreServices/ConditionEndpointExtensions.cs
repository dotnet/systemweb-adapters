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

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ConditionalRouteAttribute : Attribute
{
}

/// <summary>
/// An interface that determines if an endpoint should be enabled.
/// </summary>
public interface IConditionalEndpointSelector
{
    ValueTask<bool> IsEnabledAsync(HttpContext context, Endpoint candidate);
}

/// <summary>
/// Extension methods that enable optionally conditional endpoints
/// </summary>
public static class ConditionalEndpointExtensions
{
    /// <summary>
    /// Registers a <see cref="IConditionalEndpointSelector"/> that will be called when selecting
    /// an endpoint that has been marked as conditional by <see cref="WithConditionalRoute{TBuilder}(TBuilder)"/>.
    /// </summary>
    /// <typeparam name="T">Type of selector</typeparam>
    /// <param name="services">Service collection to add to.</param>
    public static void AddConditionalEndpoints<T>(this IServiceCollection services)
        where T : class, IConditionalEndpointSelector
    {
        services.AddTransient<IConditionalEndpointSelector, T>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ConditionalEndpointMatcherPolicy>());
    }

    /// <summary>
    /// Enable conditional behavior for supplied endpoints. An implementation of <see cref="IConditionalEndpointSelector"/> must be registered as a service for this to be enabled at runtime.
    /// </summary>
    /// <param name="builder">The endpoint convention builder</param>
    /// <returns>The original convention builder.</returns>
    public static TBuilder WithConditionalRoute<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithMetadata(new ConditionalRouteAttribute());
    }

    private sealed class ConditionalEndpointMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private readonly IConditionalEndpointSelector _selector;

        public ConditionalEndpointMatcherPolicy(IConditionalEndpointSelector selector)
        {
            _selector = selector;
        }

        public override int Order => 0;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
            => endpoints.Any(e => e.Metadata.GetMetadata<ConditionalRouteAttribute>() is not null);

        public async Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                var endpoint = candidates[i].Endpoint;

                if (endpoint.Metadata.GetMetadata<ConditionalRouteAttribute>() is not null)
                {
                    if (await _selector.IsEnabledAsync(httpContext, endpoint) == false)
                    {
                        candidates.SetValidity(i, false);
                    }
                }
            }
        }
    }
}
