// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.Extensions.Configuration;
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
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, HostingEnvironmentStartupFilter>());

        services.AddOptions<SystemWebAdaptersOptions>()

            // This configures for anyone using older IIS modules that don't set the values (and to maintain behavior with the adapters <1.3)
            .Configure(options =>
            {
                options.IsHosted = true;

                if (NativeMethods.IsAspNetCoreModuleLoaded())
                {
                    var config = NativeMethods.HttpGetApplicationProperties();

                    options.AppDomainAppVirtualPath = config.pwzVirtualApplicationPath;
                    options.AppDomainAppPath = config.pwzFullApplicationPath;
                }
            })

            // On .NET 8+, IIISEnvironmentFeature is available by default if running on IIS. We have an internal version
            // we load at startup so that regardless of version and server this may be available (for example, in case some
            // one wants to set the environment variables on a Kestrel hosted system to get the behavior)
            .Configure<IServer>((options, server) =>
            {
                if (server.Features.Get<IIISEnvironmentFeature>() is { } feature)
                {
                    options.AppDomainAppPath = feature.ApplicationPhysicalPath;
                    options.AppDomainAppVirtualPath = feature.ApplicationVirtualPath;
                    options.ApplicationID = feature.ApplicationId;
                    options.SiteName = feature.SiteName;
                }
            });
    }

    private sealed class HostingEnvironmentStartupFilter : IStartupFilter
    {
        public HostingEnvironmentStartupFilter(HostingEnvironmentAccessor accessor)
        {
            // We don't need to store this as it will remain in the DI container. However, we force it to be injected here
            // so that it will be activated early on in the pipeline and set the current runtime. When the host is completed,
            // it will be disposed and unset itself from the System.Web runtime.
            _ = accessor;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                // We ensure this feature is available for both pre-.NET 8 as well as .NET 8+ on non-IIS systems if the right environment variables are set
                if (builder.ApplicationServices.GetService<IServer>() is { } server && server.Features.Get<IIISEnvironmentFeature>() is null)
                {
                    if (IISEnvironmentFeature.TryCreate(builder.ApplicationServices.GetRequiredService<IConfiguration>(), out var feature))
                    {
                        server.Features.Set<IIISEnvironmentFeature>(feature);
                    }
                }

                next(builder);
            };
    }

    /// <summary>
    /// Copy of https://github.com/dotnet/aspnetcore/blob/4218bd758012820a955b0185e5b1824168d00c6a/src/Servers/IIS/IIS/src/Core/IISEnvironmentFeature.cs
    /// </summary>
    private sealed class IISEnvironmentFeature : IIISEnvironmentFeature
    {
        public static bool TryCreate(IConfiguration configuration, [NotNullWhen(true)] out IIISEnvironmentFeature? result)
        {
            var feature = new IISEnvironmentFeature(configuration);

            if (feature.IISVersion is not null)
            {
                result = feature;
                return true;
            }

            result = null;
            return false;
        }

        private IISEnvironmentFeature(IConfiguration configuration)
        {
            if (Version.TryParse(configuration["IIS_VERSION"], out var version))
            {
                IISVersion = version;
            }

            if (uint.TryParse(configuration["IIS_SITE_ID"], out var siteId))
            {
                SiteId = siteId;
            }

            AppPoolId = configuration["IIS_APP_POOL_ID"] ?? string.Empty;
            AppPoolConfigFile = configuration["IIS_APP_POOL_CONFIG_FILE"] ?? string.Empty;
            AppConfigPath = configuration["IIS_APP_CONFIG_PATH"] ?? string.Empty;
            ApplicationPhysicalPath = configuration["IIS_PHYSICAL_PATH"] ?? string.Empty;
            ApplicationVirtualPath = configuration["IIS_APPLICATION_VIRTUAL_PATH"] ?? string.Empty;
            ApplicationId = configuration["IIS_APPLICATION_ID"] ?? string.Empty;
            SiteName = configuration["IIS_SITE_NAME"] ?? string.Empty;
        }

        public Version IISVersion { get; } = null!;

        public string AppPoolId { get; }

        public string AppPoolConfigFile { get; }

        public string AppConfigPath { get; }

        public string ApplicationPhysicalPath { get; }

        public string ApplicationVirtualPath { get; }

        public string ApplicationId { get; }

        public string SiteName { get; }

        public uint SiteId { get; }
    }
}
