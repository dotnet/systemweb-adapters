// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Caching;
using System.Web.Util;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Hosting;

public static class HostingEnvironment
{
    public static string ApplicationID => HostingEnvironmentAccessor.Current.Options.ApplicationID;

    public static bool IsHosted => HostingEnvironmentAccessor.TryGet(out var current) && current.Options.IsHosted;

    public static string ApplicationPhysicalPath => HostingEnvironmentAccessor.Current.Options.ApplicationPhysicalPath;

    public static string ApplicationVirtualPath => HostingEnvironmentAccessor.Current.Options.ApplicationVirtualPath;

    public static string SiteName => HostingEnvironmentAccessor.Current.Options.SiteName;

    public static VirtualPathProvider? VirtualPathProvider => HostingEnvironmentAccessor.Current.Options.VirtualPathProvider;

    public static void RegisterVirtualPathProvider(VirtualPathProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        HostingEnvironmentAccessor.Current.Options.VirtualPathProvider = provider;
    }

    public static string MapPath(string? virtualPath)
    {
        if (string.IsNullOrEmpty(virtualPath))
        {
            throw new ArgumentNullException(nameof(virtualPath));
        }

        // original implementation disallows paths that are not virtual or do not begin with a forward slash, e.g. file.txt.
        if (!VirtualPathUtilityImpl.IsAppRelative(virtualPath) && UrlPath.FixVirtualPathSlashes(virtualPath)[0] != '/')
        {
            throw new ArgumentException($"The relative virtual path '{virtualPath}' is not allowed here.");
        }

        // original implementation allows paths starting with // and \\ but MapPathUtility.MapPath() throws
        // an error that the path is a physical path.  to avoid the error, collapsing multiple leading slash characters
        // to a single slash.
        if (UrlPath.IsUncSharePath(virtualPath))
        {
            virtualPath = $"/{virtualPath.TrimStart('/', '\\')}";
        }

        return HttpRuntime.WebObjectActivator.GetRequiredService<IMapPathUtility>().MapPath("/", virtualPath);
    }

    public static Cache Cache => HttpRuntime.Cache;
}
