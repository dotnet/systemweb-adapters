// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface ISetThreadCurrentPrincipal
{
    bool IsEnabled { get; }
}
