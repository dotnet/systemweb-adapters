// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Instead of this, please use <see cref="System.Web.SessionState.SessionStateBehavior"/>.
/// </summary>
[Obsolete("Prefer System.Web.SessionState")]
public enum SessionBehavior
{
    /// <summary>
    /// No session will be available on the endpoint.
    /// </summary>
    None,

    /// <summary>
    /// Asynchronously loads the session for controllers with this attribute before running the controller.
    /// </summary>
    Preload,

    /// <summary>
    /// Synchronously loads the session for controllers with this attribute on first use.
    /// </summary>
    [Obsolete("This will enable session on the endpoint but will resort to sync over async behavior")]
    OnDemand,
}
