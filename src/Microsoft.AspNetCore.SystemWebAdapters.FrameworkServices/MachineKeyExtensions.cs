// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Web.Configuration;

namespace System.Web;

internal static class MachineKeyExtensions
{
    private static readonly FieldInfo _configField = GetRequiredField("s_config");
    private static readonly MethodInfo _ensureConfig = GetRequiredMethod("EnsureConfig");
    private static readonly MethodInfo _getApplicationConfig = GetRequiredMethod("GetApplicationConfig");

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
