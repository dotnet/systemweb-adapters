// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class InMemoryLockedSessions : ILockedSessionCache
{
    private readonly ISessionSerializer _serializer;
    private readonly ConcurrentDictionary<string, SessionContainer> _cache = new();

    public InMemoryLockedSessions(ISessionSerializer serializer)
    {
        _serializer = serializer;
    }

    public IDisposable Register(HttpSessionStateBase session, Action callback)
    {
        var container = new SessionContainer(session, this, callback);

        _cache.TryAdd(session.SessionID, container);

        return container;
    }

    public async Task<SessionSaveResult> SaveAsync(string sessionId, Stream stream, CancellationToken token)
    {
        if (!_cache.TryGetValue(sessionId, out var item))
        {
            return SessionSaveResult.SessionNotFound;
        }

        try
        {
            if (item.Session is { } session)
            {
                await _serializer.DeserializeToAsync(stream, session, token);
                return SessionSaveResult.Success;
            }
            else
            {
                return SessionSaveResult.AlreadyUpdated;
            }
        }
        catch (JsonException)
        {
            return SessionSaveResult.DeserializationError;
        }
        finally
        {
            item.TryComplete();
        }
    }

    private sealed class SessionContainer : IDisposable
    {
        private readonly string _id;
        private readonly Action _callback;
        private readonly InMemoryLockedSessions _sessions;

        public SessionContainer(HttpSessionStateBase state, InMemoryLockedSessions sessions, Action callback)
        {
            _id = state.SessionID;
            _callback = callback;
            _sessions = sessions;

            Session = state;
        }

        public HttpSessionStateBase? Session { get; private set; }

        public void TryComplete()
        {
            if (Session is not null)
            {
                _callback();
                Session = null;
            }
        }

        public void Dispose() => _sessions._cache.TryRemove(_id, out _);
    }
}
