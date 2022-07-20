// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class RemoteSessionState : DelegatingSessionState
{
    private HttpResponseMessage? _response;
    private Func<ISessionState?, CancellationToken, Task>? _onCommit;

    public RemoteSessionState(ISessionState other, HttpResponseMessage response, Func<ISessionState?, CancellationToken, Task> onCommit)
    {
        State = other;
        _response = response;
        _onCommit = onCommit;
    }

    protected override ISessionState State { get; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && _response is not null)
        {
            _response.Dispose();
            _response = null;
        }
    }

    public override async Task CommitAsync(CancellationToken token)
    {
        if (_onCommit is { } onCommit)
        {
            _onCommit = null;
            await onCommit(State, token);
        }
    }
}

