// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

internal class AspNetCoreSessionManager : ISessionManager
{
    private readonly ISessionSerializer _serializer;

    public AspNetCoreSessionManager(ISessionSerializer serializer)
    {
        _serializer = serializer;
    }

    public Task<ISessionState> CreateAsync(HttpContextCore context, ISessionMetadata metadata)
        => Task.FromResult<ISessionState>(new AspNetCoreSessionState(context.Session, _serializer, metadata.IsReadOnly));
}
