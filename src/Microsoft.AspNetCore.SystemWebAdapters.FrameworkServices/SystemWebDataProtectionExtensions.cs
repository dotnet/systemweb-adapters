// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Configuration;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
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

        if (ConfigurationManager.GetSection("system.web/machineKey") is MachineKeySection section)
        {
            if (!string.IsNullOrEmpty(section.DataProtectorType))
            {
                throw new InvalidOperationException("Could not setup DataProtection for use with MachineKey due to existing configuration of system.web/machineKey");
            }
        }

        // We use this to auto-start it ASAP to ensure the dataprotector is set up before anyone else tries to do anything
        builder.Services.AddHostedService<DataProtectionInitializerStartup>();
        builder.Services.TryAddSingleton<IApplicationDiscriminator, SystemWebApplicationDiscriminator>();

        return builder.Services.AddDataProtection();
    }

    // This is used to initialized the data protection infrastructure early in the set up
    private sealed class DataProtectionInitializerStartup(IServiceProvider sp, IHostEnvironment env) : IHostedService
    {
        private static readonly FieldInfo _configField = GetRequiredField("s_config");
        private static readonly MethodInfo _getApplicationConfig = GetRequiredMethod("GetApplicationConfig");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Initialize();

            if (env.IsDevelopment())
            {
                ValidateMachineKey();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static void Initialize()
        {
            var existing = GetApplicationConfig();
            var updated = new MachineKeySection()
            {
                ApplicationName = existing.ApplicationName,
                CompatibilityMode = MachineKeyCompatibilityMode.Framework45,
                DataProtectorType = typeof(CompatibilityDataProtector).AssemblyQualifiedName,
                Decryption = existing.Decryption,
                DecryptionKey = existing.DecryptionKey,
            };

            Value = updated;

            // Force MachineKey to start up data protection
            _ = MachineKey.Protect([]);
        }

        // This is a mini test in production when under Development to verify that things are setup correctly
        private void ValidateMachineKey()
        {
            using var rng = RandomNumberGenerator.Create();

            // Arrange
            var dp = sp.GetDataProtector("User.MachineKey.Protect");
            var bytes = new byte[10];
            rng.GetBytes(bytes);

            // Act
            var dpProtected = dp.Protect(bytes);
            var mProtected = MachineKey.Protect(bytes);

            // Assert
            var unprotected1 = MachineKey.Unprotect(dpProtected);
            var unprotected2 = dp.Unprotect(mProtected);

            if (!bytes.SequenceEqual(unprotected1) || !bytes.SequenceEqual(unprotected2))
            {
                throw new InvalidOperationException("DataProtection was not setup correctly for MachineKey");
            }
        }

        private static MachineKeySection Value
        {
            get => (MachineKeySection)_configField.GetValue(null);
            set => _configField.SetValue(null, value);
        }

        private static MachineKeySection GetApplicationConfig() => (MachineKeySection)_getApplicationConfig.Invoke(null, []);

        private static FieldInfo GetRequiredField(string name)
        {
            var field = typeof(MachineKeySection).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);

            return field ?? throw new NotSupportedException($"The required MachineKeySection field '{name}' could not be found. The current System.Web implementation is not supported.");
        }

        private static MethodInfo GetRequiredMethod(string name)
        {
            var method = typeof(MachineKeySection).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

            return method ?? throw new NotSupportedException($"The required MachineKeySection method '{name}' could not be found. The current System.Web implementation is not supported.");
        }
    }

    private sealed class SystemWebApplicationDiscriminator : IApplicationDiscriminator
    {
        private readonly Lazy<string> _lazyDiscriminator;

        public SystemWebApplicationDiscriminator(IHostEnvironment env)
        {
            _lazyDiscriminator = new Lazy<string>(() => GetAppDiscriminatorCore(env));
            IsInitialized = true;
        }

        internal static bool IsInitialized { get; set; }

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
                var a = env.ApplicationName;
                var b = HostingEnvironment.SiteName;
                return env.ApplicationName;
            }
        }
    }
}
