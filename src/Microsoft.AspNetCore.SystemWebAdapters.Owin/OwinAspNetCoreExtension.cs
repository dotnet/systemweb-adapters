// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Owin;

namespace Microsoft.Extensions.DependencyInjection;

public static class OwinAspNetCoreExtension
{
    /// <summary>
    /// Add an OWIN pieline that will be executed within the emulated HttpApplication eventing similar to OWIN on ASP.NET Framework.
    /// </summary>
    public static ISystemWebAdapterBuilder AddOwinApp(this ISystemWebAdapterBuilder builder, Action<IAppBuilder> configure)
        => builder.AddOwinApp((app, _) => configure(app));

    /// <summary>
    /// Add an OWIN pieline that will be executed within the emulated HttpApplication eventing similar to OWIN on ASP.NET Framework.
    /// </summary>
    public static ISystemWebAdapterBuilder AddOwinApp(this ISystemWebAdapterBuilder builder, Action<IAppBuilder, IServiceProvider> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddHttpApplication();
        builder.Services.Configure<OwinAppOptions>(options => options.Configure += configure);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<HttpApplicationOptions>, IntegratedPipelineConfigureOptions>());

        return builder;
    }
}
