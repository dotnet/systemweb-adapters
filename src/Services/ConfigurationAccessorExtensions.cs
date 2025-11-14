// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SystemWebAdapters.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class ConfigurationAccessorExtensions
{
    public static void AddConfigurationAccessor(this IServiceCollection services)
    {
        services.TryAddSingleton<IConfigurationAccessor, ConfigurationAccessor>();
    }

    private sealed class ConfigurationAccessor(IConfiguration config) : IConfigurationAccessor
    {
        public string? GetConnectionString(string name) => config.GetConnectionString(name);

        public string? GetSetting(string key) => config[key];
    }
}
