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
        private static class PayloadKind
        {
            // V2
            public const byte Value = 1;
            public const byte Removed = 2;
            public const byte SupportedVersion = 0XFE;

            // Used to mark the end of the payload
            public const byte EndSentinel = 0xFF;
        }

        public void Write(ISessionState state, BinaryWriter writer)
        {
            writer.Write(Version2);
            writer.Write(state.SessionID);

            writer.Write(state.IsNewSession);
            writer.Write(state.IsAbandoned);
            writer.Write(state.IsReadOnly);

            writer.Write7BitEncodedInt(state.Timeout);

            foreach (var key in state.Keys)
            {
                if (serializer.TrySerialize(key, state[key], out var result))
                {
                    writer.Write(PayloadKind.Value);
                    writer.Write(key);
                    writer.Write7BitEncodedInt(result.Length);
                    writer.Write(result);
                }
            }

            writer.Write(PayloadKind.SupportedVersion);
            writer.Write(Version2);

            writer.Write(PayloadKind.EndSentinel);
        }

        public void Write(ISessionStateChangeset state, BinaryWriter writer)
        {
            writer.Write(Version2);
            writer.Write(state.SessionID);

            writer.Write(state.IsNewSession);
            writer.Write(state.IsAbandoned);
            writer.Write(state.IsReadOnly);

            writer.Write7BitEncodedInt(state.Timeout);

            foreach (var item in state.Changes)
            {
                if (item.State is SessionItemChangeState.NoChange)
                {
                    continue;
                }
                else if (item.State is SessionItemChangeState.Removed)
                {
                    writer.Write(PayloadKind.Removed);
                    writer.Write(item.Key);
                }
                else if (item.State is SessionItemChangeState.New or SessionItemChangeState.Changed && serializer.TrySerialize(item.Key, state[item.Key], out var result))
                {
                    writer.Write(PayloadKind.Value);
                    writer.Write(item.Key);
                    writer.Write7BitEncodedInt(result.Length);
                    writer.Write(result);
                }
            }

            writer.Write(PayloadKind.SupportedVersion);
            writer.Write(Version2);

            writer.Write(PayloadKind.EndSentinel);
        }

        public SessionStateCollection Read(BinaryReader reader)
        {
            var state = SessionStateCollection.CreateTracking(serializer);

            state.SessionID = reader.ReadString();
            state.IsNewSession = reader.ReadBoolean();
            state.IsAbandoned = reader.ReadBoolean();
            state.IsReadOnly = reader.ReadBoolean();
            state.Timeout = reader.Read7BitEncodedInt();

            while (true)
            {
                var kind = reader.ReadByte();

                if (kind == PayloadKind.EndSentinel)
                {
                    break;
                }

                if (kind is PayloadKind.Removed)
                {
                    var key = reader.ReadString();
                    state.MarkRemoved(key);
                }
                else if (kind is PayloadKind.Value)
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
                        throw new InvalidOperationException($"Unknown session serialization kind '{kind}'");
                    }
                }
                else if (kind is PayloadKind.SupportedVersion)
                {
                    var version = reader.ReadByte();
                }
            }

            return state;
        }
    }
}
