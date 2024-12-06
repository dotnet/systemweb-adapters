// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

[DebuggerDisplay("{State}: {Key,nq}")]
public readonly struct SessionStateChangeItem(SessionItemChangeState state, string key) : IEquatable<SessionStateChangeItem>
{
    public SessionItemChangeState State => state;

    public string Key => key;

    public override bool Equals(object? obj) => obj is SessionStateChangeItem item && Equals(item);

    public override int GetHashCode()
        => State.GetHashCode() ^ Key.GetHashCode();

    public bool Equals(SessionStateChangeItem other) =>
        State == other.State
        && string.Equals(Key, other.Key, StringComparison.Ordinal);

    public static bool operator ==(SessionStateChangeItem left, SessionStateChangeItem right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SessionStateChangeItem left, SessionStateChangeItem right)
    {
        return !(left == right);
    }
}
