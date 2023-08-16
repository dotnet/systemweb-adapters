// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdaptersOptions
{
    public string ApplicationID { get; set; } = string.Empty;

    public bool IsHosted { get; set; }

    public string SiteName { get; set; } = string.Empty;

    public string AppDomainAppVirtualPath { get; set; } = "/";

    public string AppDomainAppPath { get; set; } = AppContext.BaseDirectory;
}

#endif
