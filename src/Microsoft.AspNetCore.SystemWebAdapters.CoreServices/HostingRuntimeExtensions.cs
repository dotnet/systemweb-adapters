// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class HostingRuntimeExtensions
{
    private static readonly Type? RuntimeIIISEnvironmentFeatureType = Type.GetType("Microsoft.AspNetCore.Server.IIS.IIISEnvironmentFeature, Microsoft.AspNetCore.Server.IIS");

    private static readonly MethodInfo? RuntimeFeatureProxyCreateMethod = CreateRuntimeFeatureProxyCreateMethod();

    private static MethodInfo? CreateRuntimeFeatureProxyCreateMethod()
    {
        if (RuntimeIIISEnvironmentFeatureType is null) return null;

        try
        {
            var createMethod = typeof(DispatchProxy)
                .GetMethod(nameof(DispatchProxy.Create), BindingFlags.Public | BindingFlags.Static);

            if (createMethod is null) return null;

            return createMethod.MakeGenericMethod(RuntimeIIISEnvironmentFeatureType, typeof(RuntimeFeatureProxy));
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static void AddHostingRuntime(this IServiceCollection services)
    {
        services.TryAddSingleton<HostingEnvironmentAccessor>();
        services.TryAddSingleton<VirtualPathUtilityImpl>();
        services.TryAddSingleton<IMapPathUtility, MapPathUtility>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, HostingEnvironmentStartupFilter>());

        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<SystemWebAdaptersOptions>, OlderIISModuleSupport>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<SystemWebAdaptersOptions>, EnvironmentFeatureConfigureOptions>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<SystemWebAdaptersOptions>, DefaultAppPathConfigureOptions>());
    }

    /// <summary>
    /// On .NET 8+, IIISEnvironmentFeature is available by default if running on IIS. We have an internal version
    /// we load at startup so that regardless of version and server this may be available (for example, in case some
    /// one wants to set the environment variables on a Kestrel hosted system to get the behavior)
    /// </summary>
    private sealed class EnvironmentFeatureConfigureOptions(IServer server) : IConfigureOptions<SystemWebAdaptersOptions>
    {
        public void Configure(SystemWebAdaptersOptions options)
        {
            if (TryGetEnvironmentFeature(server, out var feature))
            {
                options.ApplicationPhysicalPath = feature.ApplicationPhysicalPath;
                options.ApplicationVirtualPath = feature.ApplicationVirtualPath;
                options.ApplicationID = feature.ApplicationId;
                options.SiteName = feature.SiteName;
            }
        }
    }

    /// <summary>
    /// On ASP.NET Core this should be the same. We're doing it here rather than a PostConfigure because someone may want to set it up differently
    /// </summary>
    private sealed class DefaultAppPathConfigureOptions : IConfigureOptions<SystemWebAdaptersOptions>
    {
        public void Configure(SystemWebAdaptersOptions options)
        {
            options.AppDomainAppPath = options.ApplicationPhysicalPath;
            options.AppDomainAppVirtualPath = options.ApplicationVirtualPath;
        }
    }

    /// <summary>
    /// This configures for anyone using older IIS modules that don't set the values (and to maintain behavior with the adapters <1.3)
    /// </summary>
    private sealed class OlderIISModuleSupport : IConfigureOptions<SystemWebAdaptersOptions>
    {
        public void Configure(SystemWebAdaptersOptions options)
        {
            options.IsHosted = true;

            if (NativeMethods.IsAspNetCoreModuleLoaded())
            {
                var config = NativeMethods.HttpGetApplicationProperties();

                options.ApplicationPhysicalPath = config.pwzFullApplicationPath;
                options.ApplicationVirtualPath = config.pwzVirtualApplicationPath;
            }
        }
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
                if (builder.ApplicationServices.GetService<IServer>() is { } server)
                {
                    // Ensure our internal feature type is set so it can be accessed regardless of runtime packaging
                    if (server.Features.Get<IIISEnvironmentFeature>() is null)
                    {
                        if (RuntimeIIISEnvironmentFeatureType is { } runtimeType && server.Features[runtimeType] is { } runtimeFeature)
                        {
                            // Runtime IIS type is set (e.g., running on IIS) - bridge it to our internal type
                            server.Features.Set<IIISEnvironmentFeature>(new RuntimeIIISEnvironmentFeature(runtimeFeature));
                        }
                        else if (IISEnvironmentFeature.TryCreate(builder.ApplicationServices.GetRequiredService<IConfiguration>(), out var feature))
                        {
                            // No IIS server feature - populate from config environment variables
                            server.Features.Set<IIISEnvironmentFeature>(feature);
                        }
                    }

                    // Ensure the runtime type is also set when our internal type is populated and the runtime type exists
                    if (RuntimeIIISEnvironmentFeatureType is { } rType
                        && server.Features[rType] is null
                        && server.Features.Get<IIISEnvironmentFeature>() is { } internalFeature)
                    {
                        TrySetRuntimeFeature(server, rType, internalFeature);
                    }
                }

                next(builder);
            };
    }

    private static bool TryGetEnvironmentFeature(IServer server, [NotNullWhen(true)] out IIISEnvironmentFeature? feature)
    {
        if (server.Features.Get<IIISEnvironmentFeature>() is { } existing)
        {
            feature = existing;
            return true;
        }

        if (RuntimeIIISEnvironmentFeatureType is { } type && server.Features[type] is { } runtimeFeature)
        {
            feature = new RuntimeIIISEnvironmentFeature(runtimeFeature);
            return true;
        }

        feature = null;
        return false;
    }

    private static void TrySetRuntimeFeature(IServer server, Type runtimeType, IIISEnvironmentFeature source)
    {
        if (RuntimeFeatureProxyCreateMethod is not { } createMethod) return;

        try
        {
            var proxy = createMethod.Invoke(null, null);

            if (proxy is not RuntimeFeatureProxy runtimeProxy) return;

            runtimeProxy.Source = source;
            server.Features[runtimeType] = proxy;
        }
        catch (TargetInvocationException)
        {
            // If proxy creation fails for any reason, continue without surfacing the runtime type
        }
        catch (MethodAccessException)
        {
            // If proxy creation fails for any reason, continue without surfacing the runtime type
        }
        catch (InvalidOperationException)
        {
            // If proxy creation fails for any reason, continue without surfacing the runtime type
        }
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

    private sealed class RuntimeIIISEnvironmentFeature : IIISEnvironmentFeature
    {
        public RuntimeIIISEnvironmentFeature(object feature)
        {
            var properties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

            foreach (var property in feature.GetType().GetProperties())
            {
                properties[property.Name] = property;
            }

            IISVersion = GetVersionProperty(feature, properties, nameof(IISVersion));
            AppPoolId = GetStringProperty(feature, properties, nameof(AppPoolId));
            AppPoolConfigFile = GetStringProperty(feature, properties, nameof(AppPoolConfigFile));
            AppConfigPath = GetStringProperty(feature, properties, nameof(AppConfigPath));
            ApplicationPhysicalPath = GetStringProperty(feature, properties, nameof(ApplicationPhysicalPath));
            ApplicationVirtualPath = GetStringProperty(feature, properties, nameof(ApplicationVirtualPath));
            ApplicationId = GetStringProperty(feature, properties, nameof(ApplicationId));
            SiteName = GetStringProperty(feature, properties, nameof(SiteName));
            SiteId = GetUIntProperty(feature, properties, nameof(SiteId));
        }

        public Version IISVersion { get; }

        public string AppPoolId { get; }

        public string AppPoolConfigFile { get; }

        public string AppConfigPath { get; }

        public string ApplicationPhysicalPath { get; }

        public string ApplicationVirtualPath { get; }

        public string ApplicationId { get; }

        public string SiteName { get; }

        public uint SiteId { get; }

        private static string GetStringProperty(object feature, IReadOnlyDictionary<string, PropertyInfo> properties, string propertyName)
            => GetPropertyValue(feature, properties, propertyName) as string ?? string.Empty;

        private static Version GetVersionProperty(object feature, IReadOnlyDictionary<string, PropertyInfo> properties, string propertyName)
            => GetPropertyValue(feature, properties, propertyName) as Version ?? new Version(0, 0);

        private static uint GetUIntProperty(object feature, IReadOnlyDictionary<string, PropertyInfo> properties, string propertyName)
            => GetPropertyValue(feature, properties, propertyName) is uint value ? value : 0;

        private static object? GetPropertyValue(object feature, IReadOnlyDictionary<string, PropertyInfo> properties, string propertyName)
            => properties.TryGetValue(propertyName, out var property) ? property.GetValue(feature) : null;
    }

    /// <summary>
    /// A DispatchProxy-based adapter that implements the runtime IIS feature type by delegating to our internal <see cref="IIISEnvironmentFeature"/>.
    /// This allows consumers that use <c>Microsoft.AspNetCore.Server.IIS.IIISEnvironmentFeature</c> directly to find the feature on the server
    /// even when it was populated via configuration environment variables rather than the IIS server.
    /// </summary>
    private sealed class RuntimeFeatureProxy : DispatchProxy
    {
        internal IIISEnvironmentFeature? Source { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => targetMethod?.Name switch
            {
                "get_IISVersion" => Source?.IISVersion,
                "get_AppPoolId" => Source?.AppPoolId,
                "get_AppPoolConfigFile" => Source?.AppPoolConfigFile,
                "get_AppConfigPath" => Source?.AppConfigPath,
                "get_ApplicationPhysicalPath" => Source?.ApplicationPhysicalPath,
                "get_ApplicationVirtualPath" => Source?.ApplicationVirtualPath,
                "get_ApplicationId" => Source?.ApplicationId,
                "get_SiteName" => Source?.SiteName,
                "get_SiteId" => Source?.SiteId,
                _ => null,
            };
    }
}
