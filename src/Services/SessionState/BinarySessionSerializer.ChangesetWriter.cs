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
    private readonly struct ChangesetWriter(ISessionKeySerializer serializer)
    {
        public List<string>? Write(ISessionStateChangeset state, BinaryWriter writer)
        {
            writer.Write(ModeDelta);
            writer.Write(state.SessionID);

            writer.Write(state.IsNewSession);
            writer.Write(state.IsAbandoned);
            writer.Write(state.IsReadOnly);

            writer.Write7BitEncodedInt(state.Timeout);
            writer.Write7BitEncodedInt(state.Count);

            List<string>? unknownKeys = null;

            foreach (var item in state.Changes)
            {
                writer.Write(item.Key);

                // New with V2 serializer
                if (item.State is SessionItemChangeState.NoChange or SessionItemChangeState.Removed)
                {
                    writer.Write7BitEncodedInt((int)item.State);
                }
                else if (serializer.TrySerialize(item.Key, state[item.Key], out var result))
                {
                    writer.Write7BitEncodedInt((int)item.State);
                    writer.Write7BitEncodedInt(result.Length);
                    writer.Write(result);
                }
                else
                {
                    (unknownKeys ??= []).Add(item.Key);
                    writer.Write7BitEncodedInt((int)SessionItemChangeState.Unknown);
                }
            }

            writer.WriteFlags([]);

            return unknownKeys;
        }

        public SessionStateCollection Read(BinaryReader reader)
        {
            var state = SessionStateCollection.CreateTracking(serializer);

            state.SessionID = reader.ReadString();
            state.IsNewSession = reader.ReadBoolean();
            state.IsAbandoned = reader.ReadBoolean();
            state.IsReadOnly = reader.ReadBoolean();
            state.Timeout = reader.Read7BitEncodedInt();

            var count = reader.Read7BitEncodedInt();

            for (var index = count; index > 0; index--)
            {
                var key = reader.ReadString();
                var changeState = (SessionItemChangeState)reader.Read7BitEncodedInt();

                if (changeState is SessionItemChangeState.NoChange)
                {
                    state.MarkUnchanged(key);
                }
                else if (changeState is SessionItemChangeState.Removed)
                {
                    state.MarkRemoved(key);
                }
                else if (changeState is SessionItemChangeState.Unknown)
                {
                    state.AddUnknownKey(key);
                }
                else if (changeState is SessionItemChangeState.New or SessionItemChangeState.Changed)
                {
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
            }

            foreach (var (flag, payload) in reader.ReadFlags())
            {
                // No flags are currently read
            }

            return state;
        }
    }
}
