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
    private const byte ModeStateV1 = 1;
    private const byte ModeStateV2 = 2;
    private const byte ModeDelta = 3;

    private readonly IOptions<SessionSerializerOptions> _options;
    private readonly ISessionKeySerializer _serializer;
    private readonly ILogger<BinarySessionSerializer> _logger;

    public BinarySessionSerializer(ICompositeSessionKeySerializer serializer, IOptions<SessionSerializerOptions> options, ILogger<BinarySessionSerializer> logger)
    {
        _serializer = serializer;
        _options = options;
        _logger = logger;
    }

    public void Write(ISessionState state, BinaryWriter writer)
    {
        if (state is ISessionStateChangeset delta)
        {
            new ChangesetWriter(_serializer).Write(delta, writer);
        }
        else
        {
            new StateWriter(_serializer, _options.Value.EnableChangeTracking ? ModeStateV2 : ModeStateV1).Write(state, writer);
        }
    }

    public ISessionState Read(BinaryReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var version = reader.ReadByte();

        return version switch
        {
            ModeStateV1 => new StateWriter(_serializer, ModeStateV1).Read(reader),
            ModeStateV2 => new StateWriter(_serializer, ModeStateV2).Read(reader),
            ModeDelta => new ChangesetWriter(_serializer).Read(reader),
            _ => throw new InvalidOperationException("Serialized session state has unknown version.")
        };
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
