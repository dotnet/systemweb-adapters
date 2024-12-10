// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

internal sealed class HttpSessionStateBaseWrapper : ISessionState
{
    public HttpSessionStateBaseWrapper(HttpSessionStateBase state)
    {
        State = state;
    }

    public HttpSessionStateBase State { get; }

    public object? this[string key]
    {
        get => State[key];
        set => State[key] = key;
    }

    public string SessionID => State.SessionID;

    public bool IsReadOnly => State.IsReadOnly;

    public int Timeout
    {
        get => State.Timeout;
        set => State.Timeout = value;
    }

    public bool IsNewSession => State.IsNewSession;

    public int Count => State.Count;

    public bool IsSynchronized => State.IsSynchronized;

    public object SyncRoot => State.SyncRoot;

    public bool IsAbandoned
    {
        get => false;
        set
        {
            if (value)
            {
                State.Abandon();
            }
        }
    }

    public IEnumerable<string> Keys
    {
        get
        {
            // If the underlying storage mechanism is SessionStateItemCollection then there can be issues around the enumerator causing side effects.
            // There are precautions in the implementation to prevent this, but we have seen an exception around this in practice (see dotnet/systemweb-adapters#556).
            // However, there is no simple way to check if this is the case, so we'll preemptively create a copy.
            // See https://referencesource.microsoft.com/#System.Web/State/SessionStateItemCollection.cs,435 for the warnings in the implementation.
            return [.. State.Keys.Cast<string>()];
        }
    }

    public void Clear() => State.Clear();

    public Task CommitAsync(CancellationToken token) => Task.CompletedTask;

    public void Dispose()
    {
    }

    public void Remove(string key) => State.Remove(key);
}
