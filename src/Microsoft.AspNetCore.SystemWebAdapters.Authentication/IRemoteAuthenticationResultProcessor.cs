// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// An interface for types that can modify a remote authentication result after
/// it has been retrieved from a remote service. This interface allows users
/// to specify heuristics to apply to remote authentication results to clean them up
/// or modify their properties in an extensible way.
/// </summary>
public interface IRemoteAuthenticationResultProcessor
{
    /// <summary>
    /// Takes some action on the given remote authentication result, to clean it up for example.
    /// </summary>
    /// <param name="result">The remote authentication result to be processed.</param>
    /// <param name="context">The HTTP context including the HTTP request that prompted the authentication request.</param>
    Task ProcessAsync(RemoteAuthenticationResult result, HttpContext context);
}
