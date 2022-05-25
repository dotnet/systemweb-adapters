// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal sealed class UnknownSessionKeyException : InvalidOperationException
{
    public UnknownSessionKeyException(IReadOnlyCollection<string> unknownKeys)
        : base(CreateMessage(unknownKeys))
    {
        UnknownKeys = unknownKeys;
    }

    private static string CreateMessage(IReadOnlyCollection<string> unknownKeys)
        => $"Unknown session keys: '{string.Join("', '", unknownKeys)}'";

    public IReadOnlyCollection<string> UnknownKeys { get; }
}
