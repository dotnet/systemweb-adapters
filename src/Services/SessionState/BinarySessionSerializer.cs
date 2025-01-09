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
    private const byte Version1 = 1;
    private const byte Version2 = 2;

    private readonly IOptions<SessionSerializerOptions> _options;
    private readonly ISessionKeySerializer _serializer;
    private readonly ILogger<BinarySessionSerializer> _logger;

    private readonly StateWriter V1Serializer;
    private readonly ChangesetWriter V2Serializer;

    public BinarySessionSerializer(ICompositeSessionKeySerializer serializer, IOptions<SessionSerializerOptions> options, ILogger<BinarySessionSerializer> logger)
    {
        _serializer = serializer;
        _options = options;
        _logger = logger;

        V1Serializer = new StateWriter(serializer);
        V2Serializer = new ChangesetWriter(serializer);
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
            Version1 => V1Serializer.Read(reader),
            Version2 => V2Serializer.Read(reader),
            _ => throw new InvalidOperationException("Serialized session state has unknown version.")
        };
    }


    public Task<ISessionState?> DeserializeAsync(Stream stream, CancellationToken token)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        return Task.FromResult<ISessionState?>(Read(reader));
    }

    public Task SerializeAsync(ISessionState state, SessionSerializerContext context, Stream stream, CancellationToken token)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        var version = context.SupportedVersion == 0 ? Version2 : context.SupportedVersion;

        if (version == 1)
        {
            V1Serializer.Write(state, writer);
        }
        else if (version == 2)
        {
            if (state is ISessionStateChangeset changes)
            {
                V2Serializer.Write(changes, writer);
            }
            else
            {
                V2Serializer.Write(state, writer);
            }
        }
        else
        {
            throw new InvalidOperationException($"Unsupported serialization version '{version}");
        }

        return Task.CompletedTask;
    }
}
