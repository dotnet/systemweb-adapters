// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class HostingRuntimeExtensions
{
    public static void AddHostingRuntime(this IServiceCollection services)
    {
        services.TryAddSingleton<HostingEnvironmentAccessor>();
        services.TryAddSingleton<VirtualPathUtilityImpl>();
        services.TryAddSingleton<IMapPathUtility, MapPathUtility>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, HostingEnvironmentStartupFilter>());

        services.AddOptions<SystemWebAdaptersOptions>()
            .Configure(options =>
            {
                options.IsHosted = true;

                if (NativeMethods.IsAspNetCoreModuleLoaded())
                {
                    var config = NativeMethods.HttpGetApplicationProperties();

                    options.AppDomainAppVirtualPath = config.pwzVirtualApplicationPath;
                    options.AppDomainAppPath = config.pwzFullApplicationPath;
                }
            });
    }

    private sealed class HostingEnvironmentStartupFilter : IStartupFilter, IDisposable
    {
        public HostingEnvironmentStartupFilter(HostingEnvironmentAccessor accessor)
        {
            HostingEnvironmentAccessor.Current = accessor;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder => next(builder);

        public void Dispose()
        {
            HostingEnvironmentAccessor.Current = null;
        }
    }
}
