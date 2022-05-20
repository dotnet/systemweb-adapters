// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public class UnknownSessionKeyException : Exception
{
    public UnknownSessionKeyException(IReadOnlyCollection<string> unknownKeys)
        : base($"Unknown session keys: '{string.Join("', '", unknownKeys)}'")
    {
        UnknownKeys = unknownKeys;
    }

    public IReadOnlyCollection<string> UnknownKeys { get; }
}
