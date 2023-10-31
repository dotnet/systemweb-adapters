// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET7_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;

namespace Microsoft.AspNetCore.OutputCaching;

public static class HttpApplicationVaryByExtensions
{
    /// <summary>
    /// Adds an output cache policy to the base policy that will query <see cref="System.Web.HttpApplication.GetVaryByCustomString(System.Web.HttpContext, string)"/>
    /// for values to vary by with the keys supplied by the the <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="options">The <see cref="OutputCacheOptions"/> to add the base policy to.</param>
    /// <param name="keySelector">The selector for keys to query <see cref="System.Web.HttpApplication"/> given the current <see cref="HttpContextCore"/>.</param>
    public static void AddHttpApplicationBasePolicy(this OutputCacheOptions options, Func<HttpContextCore, IEnumerable<string>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(keySelector);

        options.AddBasePolicy(new HttpApplicationVaryByPolicy(keySelector));
    }

    /// <summary>
    /// Adds a collection of custom keys to query <see cref="System.Web.HttpApplication.GetVaryByCustomString(System.Web.HttpContext, string)"/> for values to vary by.
    /// </summary>
    /// <param name="builder">The <see cref="OutputCachePolicyBuilder"/> to add the values to.</param>
    /// <param name="customKeys">The custom keys to vary by value.</param>
    public static void AddHttpApplicationVaryByCustom(this OutputCachePolicyBuilder builder, params string[] customKeys)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(customKeys);

        foreach (var custom in customKeys)
        {
            builder.VaryByValue(context =>
            {
                var value = context.Features.Get<IHttpApplicationFeature>()?.Application.GetVaryByCustomString(context.AsSystemWeb(), custom);

                return new(custom, value ?? string.Empty);
            });
        }
    }

    /// <summary>
    /// Adds a named output cache policy that will query <see cref="System.Web.HttpApplication.GetVaryByCustomString(System.Web.HttpContext, string)"/>
    /// for values to vary by with the keys supplied by the the <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="options">The <see cref="OutputCacheOptions"/> to add the named policy to.</param>
    /// <param name="keySelector">The selector for keys to query <see cref="System.Web.HttpApplication"/> given the current <see cref="HttpContextCore"/>.</param>
    public static void AddHttpApplicationPolicy(this OutputCacheOptions options, string name, Func<HttpContextCore, IEnumerable<string>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(keySelector);

        options.AddPolicy(name, new HttpApplicationVaryByPolicy(keySelector));
    }

    private sealed class HttpApplicationVaryByPolicy : IOutputCachePolicy
    {
        private readonly Func<HttpContextCore, IEnumerable<string>> _keySelector;

        public HttpApplicationVaryByPolicy(Func<HttpContextCore, IEnumerable<string>> keySelector)
        {
            _keySelector = keySelector;
        }

        ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            if (context.HttpContext.Features.Get<IHttpApplicationFeature>() is { Application: { } app })
            {
                foreach (var key in _keySelector(context.HttpContext))
                {
                    context.CacheVaryByRules.VaryByValues[key] = app.GetVaryByCustomString(context.HttpContext.AsSystemWeb(), key) ?? string.Empty;
                }
            }

            return ValueTask.CompletedTask;
        }

        ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
            => ValueTask.CompletedTask;

        ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
            => ValueTask.CompletedTask;
    }
}
#endif

