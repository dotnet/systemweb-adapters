// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Web.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

internal static class ConfigurationManagerConfigExtensions
{
    public static IConfigurationBuilder AddConfigurationManager(this IConfigurationBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var initialData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (ConnectionStringSettings connStr in System.Configuration.ConfigurationManager.ConnectionStrings)
        {
            if (connStr is { Name: string key, ConnectionString: string value })
            {
                initialData.Add("ConnectionStrings:" + key, value);
            }
        }

        foreach (string appSettingKey in System.Configuration.ConfigurationManager.AppSettings.Keys)
        {
            if (System.Configuration.ConfigurationManager.AppSettings[appSettingKey] is string value)
            {
                initialData.Add(appSettingKey, value);
            }
        }

        builder.AddInMemoryCollection(initialData);

        return builder;
    }

    public static IConfigurationManager UseHostingEnvironmentFallback(this IConfigurationManager configuration)
    {
        List<KeyValuePair<string, string?>>? optionList = null;
        if (configuration[HostDefaults.ApplicationKey] is null)
        {
            optionList = new();
            optionList.Add(new KeyValuePair<string, string?>(HostDefaults.ApplicationKey, HostingEnvironment.SiteName));
        }
        if (configuration[HostDefaults.EnvironmentKey] is null && (HostingEnvironment.IsDevelopmentEnvironment || IsIISExpress()))
        {
            optionList ??= new();
            optionList.Add(new KeyValuePair<string, string?>(HostDefaults.EnvironmentKey, Environments.Development));
        }
        if (configuration[HostDefaults.ContentRootKey] is null)
        {
            optionList ??= new();
            optionList.Add(new KeyValuePair<string, string?>(HostDefaults.ContentRootKey, HostingEnvironment.ApplicationPhysicalPath));
        }
        if (optionList is not null)
        {
            configuration.AddInMemoryCollection(optionList);
        }
        static bool IsIISExpress()
        {
            using var currentProcess = Process.GetCurrentProcess();

            return string.Equals(currentProcess.ProcessName, "iisexpress", StringComparison.Ordinal);
        }

        return configuration;
    }
}
