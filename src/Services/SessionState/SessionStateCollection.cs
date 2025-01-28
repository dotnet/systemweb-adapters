// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal class SessionStateCollection : ISessionState
{
    private readonly Dictionary<string, ItemHolder> _items;

    public SessionStateCollection(ISessionKeySerializer serializer)
    {
        Serializer = serializer;
        _items = [];
    }

    protected SessionStateCollection(SessionStateCollection other)
    {
        _items = other._items;

        Serializer = other.Serializer;
        UnknownKeys = other.UnknownKeys;

        SessionID = other.SessionID;
        IsReadOnly = other.IsReadOnly;
        IsNewSession = other.IsNewSession;
        IsAbandoned = other.IsAbandoned;
        Timeout = other.Timeout;
    }

    public static SessionStateCollection CreateTracking(ISessionKeySerializer serializer)
        => new SessionStateChangeset(serializer);

    public SessionStateCollection WithTracking() => new SessionStateChangeset(this);

    public ISessionKeySerializer Serializer { get; }

    public void SetUnknownKey(string key)
    {
        (UnknownKeys ??= new()).Add(key);
        _items.Remove(key);
    }

    public void MarkUnchanged(string key) => _items[key] = ItemHolder.Unchanged();

    public void MarkRemoved(string key) => _items[key] = ItemHolder.Removed();

    public void SetData(string key, byte[] data) => _items[key] = ItemHolder.FromData(data);

    public object? this[string key]
    {
        get => _items.TryGetValue(key, out var result) ? result.GetValue(key, Serializer) : null;
        set
        {
            if (_items.TryGetValue(key, out var existing))
            {
                existing.SetValue(value);
            }
            else if (value is { })
            {
                _items[key] = ItemHolder.NewValue(value);
            }
        }
    }

    public IEnumerable<SessionStateChangeItem> Changes
    {
        get
        {
            foreach (var item in _items)
            {
                yield return new(item.Value.State, item.Key);
            }
        }
    }

    internal List<string>? UnknownKeys { get; private set; }

    public string SessionID { get; set; } = null!;

    public bool IsReadOnly { get; set; }

    public int Timeout { get; set; }

    public bool IsNewSession { get; set; }

    public int Count => _items?.Count ?? 0;

    public bool IsAbandoned { get; set; }

    bool ISessionState.IsSynchronized => false;

    object ISessionState.SyncRoot => this;

    public IEnumerable<string> Keys => _items?.Keys ?? Enumerable.Empty<string>();

    public void Clear()
    {
        List<string>? newKeys = null;

        foreach (var item in _items)
        {
            if (item.Value.IsNew)
            {
                (newKeys ??= []).Add(item.Key);
            }
            else
            {
                item.Value.SetValue(null);
            }
        }

        if (newKeys is { })
        {
            foreach (var key in newKeys)
            {
                _items.Remove(key);
            }
        }
    }

    public void Remove(string key)
    {
        if (_items.TryGetValue(key, out var existing))
        {
            if (existing.IsNew)
            {
                _items.Remove(key);
            }
            else
            {
                existing.SetValue(null);
            }
        }
    }

    Task ISessionState.CommitAsync(CancellationToken token) => Task.CompletedTask;

    void IDisposable.Dispose()
    {
    }

    private sealed class ItemHolder
    {
        private byte[]? _data;
        private object? _value;

        private ItemHolder(bool isNew = false)
        {
            IsNew = isNew;
        }

        public bool IsNew { get; }

        public SessionItemChangeState State => (IsNew, _data, _value) switch
        {
            (true, _, _) => SessionItemChangeState.New,

            // If both are null, the value has been set to null implying it no longer exists
            (_, null, null) => SessionItemChangeState.Removed,

            // If the value is set, it means it has been accessed and then potentially changed
            (_, _, { }) => SessionItemChangeState.Changed,

            // If the data is still set, then the value has not been accessed
            (_, { }, _) => SessionItemChangeState.NoChange,
        };

        public object? GetValue(string key, ISessionKeySerializer serializer)
        {
            if (_data is { } data && serializer.TryDeserialize(key, data, out var obj))
            {
                _value = obj;
                _data = null;
            }

            return _value;
        }

        internal void SetValue(object? value)
        {
            _value = value;
            _data = null;
        }

        public static ItemHolder Removed() => new();

        public static ItemHolder FromData(byte[] bytes) => new() { _data = bytes };

        public static ItemHolder FromValue(object? value) => new() { _value = value };

        public static ItemHolder NewValue(object value) => new(isNew: true) { _value = value };

        public static ItemHolder Unchanged() => new() { _data = [] };
    }

    private sealed class SessionStateChangeset : SessionStateCollection, ISessionStateChangeset
    {
        public SessionStateChangeset(ISessionKeySerializer serializer)
            : base(serializer)
        {
        }

        public SessionStateChangeset(SessionStateCollection other)
            : base(other)
        {
        }
    }
}
