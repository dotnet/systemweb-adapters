// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Adapters;

namespace Microsoft.AspNetCore.SystemWebAdapters;

#if !NET45
/// <summary>
/// Provides a static way to access application configuration settings while migrating to Microsoft.Extensions.Configuration types.
/// </summary>
public static class AppConfiguration
{
    private static IConfigurationAccessor? Configuration => HttpRuntime.WebObjectActivator?.GetService(typeof(IConfigurationAccessor)) as IConfigurationAccessor;

    /// <summary>
    /// Gets a configuration setting by key. This is intended to help migrate from System.Configuration.ConfigurationManager.AppSettings to IConfiguration.
    /// </summary>
    /// <param name="key">The key of the settings.</param>
    /// <returns>The setting if available, otherwise <c>null</c>.</returns>
    public static string? GetSetting(string key) => Configuration?.GetSetting(key);

    /// <summary>
    /// Gets a connection string by name. This is intended to help migrate from System.Configuration.ConfigurationManager.ConnectionStrings to IConfiguration.
    /// </summary>
    /// <param name="name">The name of the connection string.</param>
    /// <returns>The connection string if available, otherwise <c>null</c>.</returns>
    public static string? GetConnectionString(string name) => Configuration?.GetConnectionString(name);
}
#endif
