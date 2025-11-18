// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Configuration;
using System.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.SystemWeb;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public static class SystemWebDataProtectionExtensions
{
    private const string StartupTypeKey = "aspnet:dataProtectionStartupType";

    public static IDataProtectionBuilder AddDataProtection(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (!IsMachineKeyOverriden())
        {
            const string ExpectedSetup = """
                <machineKey compatibilityMode="Framework45" dataProtectorType="Microsoft.AspNetCore.DataProtection.SystemWeb.CompatibilityDataProtector, Microsoft.AspNetCore.DataProtection.SystemWeb" />
                """;
            throw new InvalidOperationException($"Must configure machine key in web.config to use data protection: {ExpectedSetup}");
        }

        if (!TrySetStartupType())
        {
            throw new InvalidOperationException($"Must not manually set the '{StartupTypeKey}' app setting when using HttpApplicationHostBuilder.AddDataProtection.");
        }

        return builder.Services.AddDataProtection();
    }

    private static bool TrySetStartupType()
    {
        var startupType = typeof(MachineKeyImpl).AssemblyQualifiedName;

        var current = ConfigurationManager.AppSettings[StartupTypeKey];

        if (current != startupType && !string.IsNullOrEmpty(current))
        {
            return false;
        }

        ConfigurationManager.AppSettings[StartupTypeKey] = startupType;

        return true;
    }

    private static bool IsMachineKeyOverriden()
    {
        if (ConfigurationManager.GetSection("system.web/machineKey") is MachineKeySection section)
        {
            if (section.CompatibilityMode != MachineKeyCompatibilityMode.Framework45)
            {
                return false;
            }

            if (section.DataProtectorType is { } typeString && Type.GetType(typeString) is { } type && type == typeof(CompatibilityDataProtector))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class MachineKeyImpl : DataProtectionStartup
    {
        public override IDataProtectionProvider CreateDataProtectionProvider(IServiceProvider services)
        {
            return base.CreateDataProtectionProvider(HttpApplicationHost.Current.Services);
        }
    }
}
