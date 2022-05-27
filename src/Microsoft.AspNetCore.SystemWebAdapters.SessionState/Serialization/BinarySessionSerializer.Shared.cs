// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if !NETFRAMEWORK
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#endif

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class BinarySessionSerializer : ISessionSerializer
{
    private const byte Version = 1;

    private readonly JsonSessionSerializerOptions _options;
    private readonly ISessionKeySerializer _serializer;

#if NETFRAMEWORK
    public BinarySessionSerializer(ISessionKeySerializer serializer, JsonSessionSerializerOptions options)
    {
        _serializer = serializer;
        _options = options;
    }

    partial void LogSerialization(string key);
#else
    private readonly ILogger<BinarySessionSerializer> _logger;

    public BinarySessionSerializer(ISessionKeySerializer serializer, IOptions<JsonSessionSerializerOptions> options, ILogger<BinarySessionSerializer> logger)
    {
        _serializer = serializer;
        _options = options.Value;
        _logger = logger;
    }


    [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Could not serialize unknown session key '{Key}'")]
    partial void LogSerialization(string key);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Could not deserialize unknown session key '{Key}'")]
    partial void LogDeserialization(string key);
#endif

    public void Write(ISessionState state, BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(_serializer.Id);
        writer.Write(state.SessionID);

        writer.Write(state.IsNewSession);
        writer.Write(state.IsAbandoned);
        writer.Write(state.IsReadOnly);

        writer.Write7BitEncodedInt(state.Timeout);
        writer.Write7BitEncodedInt(state.Count);

        List<string>? unknownKeys = null;

        foreach (var item in state.Keys)
        {
            writer.Write(item);

            if (state[item] is { } obj)
            {
                if (_serializer.TrySerialize(item, obj, out var result))
                {
                    writer.Write7BitEncodedInt(result.Length);
                    writer.Write(result);
                }
                else
                {
                    (unknownKeys ??= new()).Add(item);
                    writer.Write7BitEncodedInt(0);
                }
            }
            else
            {
                writer.Write7BitEncodedInt(0);
            }
        }

        if (unknownKeys is null)
        {
            writer.Write7BitEncodedInt(0);
        }
        else
        {
            writer.Write7BitEncodedInt(unknownKeys.Count);

            foreach (var key in unknownKeys)
            {
                LogSerialization(key);
                writer.Write(key);
            }
        }

        if (unknownKeys is not null && _options.ThrowOnUnknownSessionKey)
        {
            throw new UnknownSessionKeyException(unknownKeys);
        }
    }


    public ISessionState Read(BinaryReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (reader.ReadByte() != Version)
        {
            throw new InvalidOperationException("Serialized session state has different payload");
        }

        if (!string.Equals(reader.ReadString(), _serializer.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Serialized session state used a different serializer for keys");
        }

        var state = new BinaryReaderSerializedSessionState(reader, _serializer);

        if (state.UnknownKeys is { Count: > 0 } unknownKeys)
        {
#if !NETFRAMEWORK
            foreach (var unknown in unknownKeys)
            {
                LogDeserialization(unknown);
            }
#endif

            if (_options.ThrowOnUnknownSessionKey)
            {
                throw new UnknownSessionKeyException(unknownKeys);
            }
        }

        return state;
    }

    public Task<ISessionState?> DeserializeAsync(Stream stream, CancellationToken token)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        return Task.FromResult<ISessionState?>(Read(reader));
    }

    public Task SerializeAsync(ISessionState state, Stream stream, CancellationToken token)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        Write(state, writer);

        return Task.CompletedTask;
    }

    private class BinaryReaderSerializedSessionState : ISessionState
    {
        public BinaryReaderSerializedSessionState(BinaryReader reader, ISessionKeySerializer serializer)
        {
            SessionID = reader.ReadString();
            IsNewSession = reader.ReadBoolean();
            IsAbandoned = reader.ReadBoolean();
            IsReadOnly = reader.ReadBoolean();
            Timeout = reader.Read7BitEncodedInt();

            var count = reader.Read7BitEncodedInt();

            for (var index = count; index > 0; index--)
            {
                var key = reader.ReadString();
                var length = reader.Read7BitEncodedInt();
                var bytes = reader.ReadBytes(length);

                if (serializer.TryDeserialize(key, bytes, out var result))
                {
                    if (result is not null)
                    {
                        this[key] = result;
                    }
                }
                else
                {
                    (UnknownKeys ??= new()).Add(key);
                }
            }

            var unknown = reader.Read7BitEncodedInt();

            if (unknown > 0)
            {
                for (var index = unknown; index > 0; index--)
                {
                    (UnknownKeys ??= new()).Add(reader.ReadString());
                }
            }
        }

        private Dictionary<string, object?>? _items;

        public object? this[string key]
        {
            get => _items?.TryGetValue(key, out var result) is true ? result : null;
            set => (_items ??= new())[key] = value;
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

        IEnumerable<string> ISessionState.Keys => _items?.Keys ?? Enumerable.Empty<string>();

        void ISessionState.Clear() => _items?.Clear();

        void ISessionState.Remove(string key) => _items?.Remove(key);

        Task ISessionState.CommitAsync(CancellationToken token) => Task.CompletedTask;

        void IDisposable.Dispose()
        {
        }
    }
}
