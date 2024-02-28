// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Caching;

namespace System.Web.Hosting;

public static class HostingEnvironment
{
    public static string ApplicationID => HostingEnvironmentAccessor.Current.Options.ApplicationID;

    public static bool IsHosted => HostingEnvironmentAccessor.TryGet(out var current) && current.Options.IsHosted;

    public static string ApplicationPhysicalPath => HostingEnvironmentAccessor.Current.Options.ApplicationPhysicalPath;

    public static string ApplicationVirtualPath => HostingEnvironmentAccessor.Current.Options.ApplicationVirtualPath;

    public static string SiteName => HostingEnvironmentAccessor.Current.Options.SiteName;

    public static Cache Cache => HttpRuntime.Cache;
}
