// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Options for connecting to a remote app.
/// </summary>
public class RemoteAppServerOptions
{
    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    [Required]
    public string ApiKeyHeader { get; set; } = RemoteConstants.ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = null!;
}
