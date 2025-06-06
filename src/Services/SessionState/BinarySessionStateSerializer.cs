// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal readonly struct BinarySessionStateSerializer(ISessionKeySerializer serializer)
{
    public void Write(ISessionState state, BinaryWriter writer)
    {
        writer.Write(BinarySessionSerializer.Version1);
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

            if (serializer.TrySerialize(item, state[item], out var result))
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

        if (unknownKeys is null)
        {
            writer.Write7BitEncodedInt(0);
        }
        else
        {
            writer.Write7BitEncodedInt(unknownKeys.Count);

            foreach (var key in unknownKeys)
            {
                writer.Write(key);
            }
        }
    }

    public SessionStateCollection Read(BinaryReader reader)
    {
        var state = new SessionStateCollection(serializer);

        state.SessionID = reader.ReadString();
        state.IsNewSession = reader.ReadBoolean();
        state.IsAbandoned = reader.ReadBoolean();
        state.IsReadOnly = reader.ReadBoolean();
        state.Timeout = reader.Read7BitEncodedInt();

        var count = reader.Read7BitEncodedInt();

        for (var index = count; index > 0; index--)
        {
            var key = reader.ReadString();
            var length = reader.Read7BitEncodedInt();
            var bytes = reader.ReadBytes(length);

            state.SetData(key, bytes);
        }

        var unknown = reader.Read7BitEncodedInt();

        if (unknown > 0)
        {
            for (var index = unknown; index > 0; index--)
            {
                state.SetUnknownKey(reader.ReadString());
            }
        }

        return state;
    }
}
