// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Hosting;

public static class HostingEnvironment
{
    public static string ApplicationID => HostingEnvironmentAccessor.Current.Options.ApplicationID;

    public static bool IsHosted => HostingEnvironmentAccessor.TryGet(out var current) && current.Options.IsHosted;

    public static string SiteName => HostingEnvironmentAccessor.Current.Options.SiteName;

    public static VirtualPathProvider? VirtualPathProvider => HostingEnvironmentAccessor.Current.Options.VirtualPathProvider;

    public static void RegisterVirtualPathProvider(VirtualPathProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var options = HostingEnvironmentAccessor.Current.Options;

        provider.Initialize(options.VirtualPathProvider);

        options.VirtualPathProvider = provider;

    }
}
