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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Source shared with .NET Framework that does not have the method")]
internal partial class BinarySessionSerializer : ISessionSerializer
{
    private const byte ModeState = 1;
    private const byte ModeDelta = 2;

    private readonly SessionSerializerOptions _options;
    private readonly ISessionKeySerializer _serializer;
    private readonly ILogger<BinarySessionSerializer> _logger;

    public BinarySessionSerializer(ICompositeSessionKeySerializer serializer, IOptions<SessionSerializerOptions> options, ILogger<BinarySessionSerializer> logger)
    {
        _serializer = serializer;
        _options = options.Value;
        _logger = logger;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Could not serialize unknown session key '{Key}'")]
    partial void LogSerialization(string key);

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Could not deserialize unknown session key '{Key}'")]
    partial void LogDeserialization(string key);

    public void Write(ISessionState state, BinaryWriter writer)
    {
        var unknownKeys = state is ISessionStateChangeset delta
            ? new ChangesetWriter(_serializer).Write(delta, writer)
            : new StateWriter(_serializer).Write(state, writer);

        if (unknownKeys is { })
        {
            foreach (var key in unknownKeys)
            {
                LogSerialization(key);
            }

            if (_options.ThrowOnUnknownSessionKey)
            {
                throw new UnknownSessionKeyException(unknownKeys);
            }
        }
    }

    public ISessionState Read(BinaryReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var version = reader.ReadByte();

        var state = version switch
        {
            ModeState => new StateWriter(_serializer).Read(reader),
            ModeDelta => new ChangesetWriter(_serializer).Read(reader),
            _ => throw new InvalidOperationException("Serialized session state has unknown version.")
        };

        if (state.UnknownKeys is { Count: > 0 } unknownKeys)
        {
            foreach (var unknown in unknownKeys)
            {
                LogDeserialization(unknown);
            }

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
}
