// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Web.Configuration;

namespace System.Web;

internal static class MachineKeyExtensions
{

    private static FieldInfo _configField = typeof(MachineKeySection).GetField("s_config", BindingFlags.NonPublic | BindingFlags.Static);
    private static MethodInfo _ensureConfig = typeof(MachineKeySection).GetMethod("EnsureConfig", BindingFlags.NonPublic | BindingFlags.Static);
    private static MethodInfo _getApplicationConfig = typeof(MachineKeySection).GetMethod("GetApplicationConfig", BindingFlags.NonPublic | BindingFlags.Static);

    extension(MachineKeySection section)
    {
        internal static MachineKeySection Value
        {
            get => (MachineKeySection)_configField.GetValue(null);
            set => _configField.SetValue(null, value);
        }

        internal static void EnsureConfig() => _ensureConfig.Invoke(null, []);

        internal static MachineKeySection GetApplicationConfig() => (MachineKeySection)_getApplicationConfig.Invoke(null, []);
    }
}
