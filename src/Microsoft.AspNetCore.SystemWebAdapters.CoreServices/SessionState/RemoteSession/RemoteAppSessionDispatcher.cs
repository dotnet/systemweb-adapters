// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// This is used to dispatch to either <see cref="DoubleConnectionRemoteAppSessionManager"/> if in read-only mode or <see cref="SingleConnectionWriteableRemoteAppSessionStateManager"/> if not.
/// </summary>
internal sealed class RemoteAppSessionDispatcher(SingleConnectionWriteableRemoteAppSessionStateManager singleConnection, DoubleConnectionRemoteAppSessionManager doubleConnection) : ISessionManager
{
    public Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        if (metadata.IsReadOnly)
        {
            // In readonly mode it's a simple GET request
            return doubleConnection.CreateAsync(context, metadata);
        }

        return singleConnection.CreateAsync(context, metadata);
    }
}
