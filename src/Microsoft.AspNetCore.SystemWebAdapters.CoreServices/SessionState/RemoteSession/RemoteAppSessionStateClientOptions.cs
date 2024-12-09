// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

public class RemoteAppSessionStateClientOptions
{
    [Required]
    public PathString SessionEndpointPath
    {
        get => Path.Path;
        set => Path = new(value);
    }

    /// <summary>
    /// Gets or sets whether a single connection should be used. If the framework deployment is the source of truth that doesn't support the single connection mode (such as it can't support HTTP2), set this to <c>false</c>.
    /// </summary>
    public bool UseSingleConnection { get; set; } = true;

    internal RelativePathString Path { get; private set; } = new(SessionConstants.SessionEndpointPath);

    /// <summary>
    /// Gets or sets the cookie name that the ASP.NET framework app is expecting to hold the session id
    /// </summary>
    [Required]
    public string CookieName { get; set; } = SessionConstants.DefaultCookieName;
}
