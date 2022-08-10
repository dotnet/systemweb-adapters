// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

public class RemoteAppSessionStateClientOptions
{
    [Required]
    public string SessionEndpointPath { get; set; } = SessionConstants.SessionEndpointPath;

    /// <summary>
    /// Gets or sets the cookie name that the ASP.NET framework app is expecting to hold the session id
    /// </summary>
    [Required]
    public string CookieName { get; set; } = SessionConstants.DefaultCookieName;
}
