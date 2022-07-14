// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class BufferResponseStreamAttribute : Attribute
{
    public bool IsDisabled { get; set; }

    public int MemoryThreshold { get; set; } = 32768; // Same default as FileBufferingWriteStream

    public long? BufferLimit { get; set; }
}
