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
    private readonly struct StateWriter(ISessionKeySerializer serializer)
    {
        private const byte FLAG_DIFF_REQUESTED = 100;

        public List<string>? Write(ISessionState state, BinaryWriter writer)
        {
            writer.Write(ModeState);
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

            // Currently only a single flag is supported, so we set the count byte to 1
            writer.Write((byte)1);

            // Let the client know that we can handle diff response
            writer.Write(FLAG_DIFF_REQUESTED);

            return unknownKeys;
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

                if (serializer.TryDeserialize(key, bytes, out var result))
                {
                    if (result is not null)
                    {
                        state[key] = result;
                    }
                }
                else
                {
                    state.AddUnknownKey(key);
                }
            }

            var unknown = reader.Read7BitEncodedInt();

            if (unknown > 0)
            {
                for (var index = unknown; index > 0; index--)
                {
                    state.AddUnknownKey(reader.ReadString());
                }
            }

            // Originally this was the end of the data. Now, we have an optional set of flags, but we can stop if there is no more data
            if (reader.PeekChar() != -1)
            {
                // Process additional data
                var length = reader.ReadByte();

                while (length > 0)
                {
                    var id = reader.ReadByte();

                    if (id == FLAG_DIFF_REQUESTED)
                    {
                        state = state.WithTracking();
                    }

                    length--;
                }
            }

            return state;
        }
    }
}
