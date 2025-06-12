// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
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

        foreach (var connStr in System.Configuration.ConfigurationManager.ConnectionStrings.OfType<IDictionaryEnumerator>())
        {
            if (connStr is { Key: string key, Value: string value })
            {
                initialData.Add("ConnectionStrings:" + key, value);
            }
        }

        foreach (var appSetting in System.Configuration.ConfigurationManager.AppSettings.OfType<IDictionaryEnumerator>())
        {
            if (appSetting is { Key: string key, Value: string value })
            {
                initialData.Add(key, value);
            }
        }

        builder.AddInMemoryCollection(initialData);

        return builder;
    }
}


