// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

public abstract class DelegatingSessionState : ISessionState
{
    private bool _disposedValue;

    protected DelegatingSessionState()
    {
    }

    protected abstract ISessionState State { get; }

    public virtual object? this[string name]
    {
        get => State[name];
        set => State[name] = value;
    }

    public virtual string SessionID => State.SessionID;

    public virtual int Count => State.Count;

    public virtual bool IsReadOnly => State.IsReadOnly;

    public virtual int Timeout
    {
        get => State.Timeout;
        set => State.Timeout = value;
    }

    public virtual bool IsNewSession => State.IsNewSession;

    public virtual bool IsSynchronized => State.IsSynchronized;

    public virtual object SyncRoot => State.SyncRoot;

    public virtual bool IsAbandoned
    {
        get => State.IsAbandoned;
        set => State.IsAbandoned = value;
    }

    public virtual void Clear() => State.Clear();

    public virtual void Remove(string key) => State.Remove(key);

    public virtual Task CommitAsync(CancellationToken token) => State.CommitAsync(token);

    public virtual IEnumerable<string> Keys => State.Keys;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                State.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
