// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

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
}


