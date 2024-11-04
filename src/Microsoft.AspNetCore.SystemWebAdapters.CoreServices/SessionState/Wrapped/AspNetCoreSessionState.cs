// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

internal sealed partial class AspNetCoreSessionState : ISessionState
{
    private readonly ISession _session;
    private readonly ISessionKeySerializer _serializer;
    private readonly ILogger<AspNetCoreSessionState> _logger;
    private readonly bool _throwOnUnknown;

    public AspNetCoreSessionState(ISession session, ISessionKeySerializer serializer, ILoggerFactory factory, bool isReadOnly, bool throwOnUnknown)
    {
        _session = session;
        _serializer = serializer;
        _throwOnUnknown = throwOnUnknown;
        _logger = factory.CreateLogger<AspNetCoreSessionState>();

        IsNewSession = !session.Keys.Any();
        IsReadOnly = isReadOnly;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Could not serialize unknown session key '{Key}'")]
    partial void LogSerialization(string key);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Could not deserialize unknown session key '{Key}'")]
    partial void LogDeserialization(string key);

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
            if (_session.Get(key) is { } value)
            {
                if (_serializer.TryDeserialize(key, value, out var result))
                {
                    return result;
                }
                else
                {
                    LogSerialization(key);

                    if (_throwOnUnknown)
                    {
                        throw new UnknownSessionKeyException(key);
                    }
                }
            }

            return null;
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
                else
                {
                    LogSerialization(key);

                    if (_throwOnUnknown)
                    {
                        throw new UnknownSessionKeyException(key);
                    }
                }
            }
        }
    }

    public string SessionID => _session.Id;

    public bool IsReadOnly { get; }

    public int Timeout { get; set; } = 20;

    public bool IsNewSession { get; }

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

