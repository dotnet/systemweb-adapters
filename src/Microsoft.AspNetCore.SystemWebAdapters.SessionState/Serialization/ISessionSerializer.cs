// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public interface ISessionSerializer
{
    ISessionState? Deserialize(string? input);

    byte[] Serialize(ISessionState state);

    byte[] Serialize(string key, object value);

    object? Deserialize(string key, Memory<byte> bytes);
}
