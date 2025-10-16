// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.Adapters;

internal interface IConfigurationAccessor
{
    string? GetSetting(string key);

    string? GetConnectionString(string name);
}
