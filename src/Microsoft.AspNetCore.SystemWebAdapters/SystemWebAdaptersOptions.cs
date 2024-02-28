// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Web;
using System.Web.Hosting;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class SystemWebAdaptersOptions
{
    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.ApplicationID"/>
    /// </summary>
    public string ApplicationID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.IsHosted"/>
    /// </summary>
    public bool IsHosted { get; set; }

    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.SiteName"/>
    /// </summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value used by <see cref="HttpRuntime.AppDomainAppVirtualPath"/>. Generally should be the same as <see cref="ApplicationVirtualPath"/> since ASP.NET Core does not have the concept of AppDomains.
    /// </summary>
    public string AppDomainAppVirtualPath { get; set; } = "/";

    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.ApplicationVirtualPath"/>.
    /// </summary>
    public string ApplicationVirtualPath { get; set; } = "/";

    /// <summary>
    /// Gets or sets the value used by <see cref="HostingEnvironment.ApplicationPhysicalPath"/>.
    /// </summary>
    public string ApplicationPhysicalPath { get; set; } = AppContext.BaseDirectory;

    /// <summary>
    /// Gets or sets the value used by <see cref="HttpRuntime.AppDomainAppPath"/>. Generally should be the same as <see cref="ApplicationPhysicalPath"/> since ASP.NET Core does not have the concept of AppDomains.
    /// </summary>
    public string AppDomainAppPath { get; set; } = AppContext.BaseDirectory;
}

#endif
