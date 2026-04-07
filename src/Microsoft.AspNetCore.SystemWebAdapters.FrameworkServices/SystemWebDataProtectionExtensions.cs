// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Configuration;
using System.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.DataProtection.SystemWeb;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace System.Web;

public static class SystemWebDataProtectionExtensions
{
    public static IDataProtectionBuilder AddDataProtection(this HttpApplicationHostBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        TrySetMachineKeyOverride();

        builder.Services.TryAddSingleton<IApplicationDiscriminator, SystemWebApplicationDiscriminator>();

        return builder.Services.AddDataProtection();
    }


    private static bool _isSet;
    private static readonly object _initializationLock = new();

    private static void TrySetMachineKeyOverride()
    {
        if (_isSet)
        {
            return;
        }

        lock (_initializationLock)
        {
            if (_isSet)
            {
                return;
            }

            if (ConfigurationManager.GetSection("system.web/machineKey") is MachineKeySection section)
            {
                if (!string.IsNullOrEmpty(section.DataProtectorType))
                {
                    throw new InvalidOperationException("Could not setup DataProtection for use with MachineKey due to existing configuration of system.web/machineKey");
                }

                if (section.CompatibilityMode != MachineKeyCompatibilityMode.Framework45)
                {
                    throw new InvalidOperationException($"Could not setup DataProtection for use with MachineKey due to invalid CompatibilityMode ({section.CompatibilityMode})");
                }
            }

            var existing = MachineKeySection.GetApplicationConfig();
            var updated = new MachineKeySection()
            {
                ApplicationName = existing.ApplicationName,
                CompatibilityMode = MachineKeyCompatibilityMode.Framework45,
                DataProtectorType = typeof(CompatibilityDataProtector).AssemblyQualifiedName,
                Decryption = existing.Decryption,
                DecryptionKey = existing.DecryptionKey,
            };

            MachineKeySection.Value = updated;

#if DEBUG
            var newAppConfig = MachineKeySection.GetApplicationConfig();
            MachineKeySection.EnsureConfig();
            var afterEnsureConfig = MachineKeySection.GetApplicationConfig();

            if (!object.ReferenceEquals(updated, newAppConfig) || !ReferenceEquals(updated, afterEnsureConfig))
            {
                throw new InvalidOperationException("MachineKey was overwritten");
            }
#endif
            _isSet = true;
        }
    }

    private sealed class SystemWebApplicationDiscriminator : IApplicationDiscriminator
    {
        private readonly Lazy<string> _lazyDiscriminator;

        public SystemWebApplicationDiscriminator(IHostEnvironment env)
        {
            _lazyDiscriminator = new Lazy<string>(() => GetAppDiscriminatorCore(env));
        }

        public string Discriminator => _lazyDiscriminator.Value;

        private static string GetAppDiscriminatorCore(IHostEnvironment env)
        {
            // Try reading the discriminator from <machineKey applicationName="..." /> defined
            // at the web app root. If the value was set explicitly (even if the value is empty),
            // honor it as the discriminator.
            var machineKeySection = (MachineKeySection)WebConfigurationManager.GetWebApplicationSection("system.web/machineKey");
            if (machineKeySection.ElementInformation.Properties["applicationName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return machineKeySection.ApplicationName;
            }
            else
            {
                return env.ApplicationName;
            }
        }
    }
}
