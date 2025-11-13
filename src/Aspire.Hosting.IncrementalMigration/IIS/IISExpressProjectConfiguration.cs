// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;

namespace Aspire.Hosting;

/// <summary>
/// This class represents the configuration for an IIS Express project.
/// </summary>
public record IISExpressProjectConfiguration
{
    /// <summary>
    /// Gets the default application pool name for IIS Express.
    /// </summary>
    public const string DefaultAppPool = "Clr4IntegratedAppPool";

    /// <summary>
    /// Gets or sets the name of the IIS Express project.
    /// </summary>
    public required string SiteName { get; init; }

    /// <summary>
    /// Gets or sets the site ID for the IIS Express project.
    /// </summary>
    public string SiteId { get; init; } = "1";

    /// <summary>
    /// Gets or sets the application pool name for the IIS Express project.
    /// </summary>
    public string AppPool { get; init; } = DefaultAppPool;

    /// <summary>
    /// Gets the application URL for the IIS Express project.
    /// </summary>
    /// <remarks>
    /// This is currently readonly as wiring it up differently than the default is not yet supported.
    /// </remarks>
    public PathString ApplicationPath { get; } = "/";

    /// <summary>
    /// Gets the virtual directory path for the IIS Express project.
    /// </summary>
    /// <remarks>
    /// This is currently readonly as wiring it up differently than the default is not yet supported.
    /// </remarks>
    public PathString VirtualDirectoryPath { get; } = "/";
}
