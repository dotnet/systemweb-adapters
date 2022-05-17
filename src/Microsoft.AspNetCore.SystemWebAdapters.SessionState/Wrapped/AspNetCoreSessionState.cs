// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

internal class AspNetCoreSessionState : ISessionState
{
    private readonly ISession _session;
    private readonly ISessionSerializer _serializer;

    public AspNetCoreSessionState(ISession session, ISessionSerializer serializer, bool isReadOnly)
    {
        _session = session;
        _serializer = serializer;

        IsReadOnly = isReadOnly;
    }

    private void CheckReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Session is readonly");
        }
    }

    public object? this[string key]
    {
        get => _serializer.Deserialize(key, _session.Get(key));
        set
        {
            CheckReadOnly();

            if (value is null)
            {
                _session.Remove(key);
            }
            else
            {
                _session.Set(key, _serializer.Serialize(key, value));
            }
        }
    }

    public string SessionID => _session.Id;

    public bool IsReadOnly { get; }

    public int Timeout { get; set; } = 20;

    public bool IsNewSession => false;

    public int Count => _session.Keys.Count();

    public bool IsSynchronized => false;

    public object SyncRoot => _session;

    public bool IsAbandoned { get; set; }

    public IEnumerable<string> Keys => _session.Keys;

    public void Clear()
    {
        CheckReadOnly();
        _session.Clear();
    }

    public async ValueTask CommitAsync(CancellationToken token)
    {
        CheckReadOnly();

        if (IsAbandoned)
        {
            _session.Clear();
        }

        await _session.CommitAsync(token);
    }

    public ValueTask DisposeAsync() => default;

    public void Remove(string key)
    {
        CheckReadOnly();
        _session.Remove(key);
    }
}

