// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

public class RemoteAppSessionStateServerOptions
{
    public string SessionEndpointPath { get; set; } = SessionConstants.SessionEndpointPath;

    /// <summary>
    /// Gets or sets the cookie name that the ASP.NET framework app is expecting to hold the session id
    /// </summary>
    public string CookieName { get; set; } = SessionConstants.DefaultCookieName;
}
