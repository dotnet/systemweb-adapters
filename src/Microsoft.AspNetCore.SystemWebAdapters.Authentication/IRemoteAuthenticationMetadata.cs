// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Metadata for determining whether the remote authentication handler should
/// be enabled for an endpoint or not. Remote authentication enables useful migration
/// scenarios but adds unnecessary performance overhead in cases where it isn't needed.
/// </summary>
public interface IRemoteAuthenticationMetadata
{
    /// <summary>
    /// Gets or sets a value indicating whether a remote app should be consulted for authentication.
    /// </summary>
    bool Enabled { get; }
}
