// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

internal sealed class AspNetCoreSessionState : ISessionState
{
    private readonly ISession _session;
    private readonly ISessionKeySerializer _serializer;

    public AspNetCoreSessionState(ISession session, ISessionKeySerializer serializer, bool isReadOnly)
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
        get
        {
            return _session.Get(key) is { } value && _serializer.TryDeserialize(key, value, out var result) ? result : null;
        }
        set
        {
            CheckReadOnly();

            if (value is null)
            {
                _session.Remove(key);
            }
            else
            {
                if (_serializer.TrySerialize(key, value, out var result))
                {
                    _session.Set(key, result);
                }
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

    public Task CommitAsync(CancellationToken token)
    {
        CheckReadOnly();

        if (IsAbandoned)
        {
            _session.Clear();
        }

        return _session.CommitAsync(token);
    }

    public void Dispose()
    {
    }

    public void Remove(string key)
    {
        CheckReadOnly();
        _session.Remove(key);
    }
}

