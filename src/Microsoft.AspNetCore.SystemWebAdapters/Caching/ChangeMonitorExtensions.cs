// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using static System.Web.Caching.CacheDependency;

namespace System.Runtime.Caching;

internal static class ChangeMonitorExtensions
{
    internal static DateTimeOffset GetLastModified(this ChangeMonitor changeMonitor) => changeMonitor switch
    {
        FileChangeMonitor fcm => fcm.LastModified,
        CacheEntryChangeMonitor cecm => cecm.LastModified,
        CacheDependencyChangeMonitor cdcm => cdcm.LastModified,
        _ => DateTimeOffset.MinValue
    };

    internal static DateTime GetLastModifiedUtc(this ChangeMonitor changeMonitor) => changeMonitor.GetLastModified().UtcDateTime;
}
