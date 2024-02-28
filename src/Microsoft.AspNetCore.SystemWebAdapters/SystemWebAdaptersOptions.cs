// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Web.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdaptersOptions
{
    private VirtualPathProvider? _virtualPathProvider;

    public string ApplicationID { get; set; } = string.Empty;

    public bool IsHosted { get; set; }

    public string SiteName { get; set; } = string.Empty;

    public string AppDomainAppVirtualPath { get; set; } = "/";

    public string AppDomainAppPath { get; set; } = AppContext.BaseDirectory;

    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.VirtualPathProvider"/>.
    /// </summary>
    public VirtualPathProvider? VirtualPathProvider
    {
        get => _virtualPathProvider;
        set
        {
            value?.Initialize(_virtualPathProvider);

            _virtualPathProvider = value;
        }
    }
}

#endif
