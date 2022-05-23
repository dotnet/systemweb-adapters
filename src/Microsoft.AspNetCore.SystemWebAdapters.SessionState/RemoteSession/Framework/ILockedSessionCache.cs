// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal interface ILockedSessionCache
{
    /// <summary>
    /// Registers a session that has been locked.
    /// </summary>
    /// <param name="session">Session that has been locked.</param>
    /// <param name="callback">Callback for when the session has been updated.</param>
    /// <returns>An <see cref="IDisposable"/> that will remove the session from the cache when disposed.</returns>
    IDisposable Register(HttpSessionStateBase session, Action callback);

    /// <summary>
    /// Updates the session for a given <paramref name="sessionId"/>.
    /// </summary>
    Task<SessionSaveResult> SaveAsync(string sessionId, Stream stream, CancellationToken token);
}
