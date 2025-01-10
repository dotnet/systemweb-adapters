// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal sealed class SessionSerializerContext(byte supportedVersion)
{
    public static SessionSerializerContext V1 { get; } = new(BinarySessionSerializer.Version1);

    public static SessionSerializerContext V2 { get; } = new(BinarySessionSerializer.Version2);

    public static SessionSerializerContext Latest => V2;

    public static SessionSerializerContext Default => V1;

    public byte SupportedVersion => supportedVersion;

    public static SessionSerializerContext Parse(IEnumerable<string> all) => all.Select(Parse).Max() ?? V1;

    public static SessionSerializerContext Parse(string? supportedVersionString) => supportedVersionString switch
    {
        "1" => V1,
        "2" => V2,
        _ => V1,
    };

    public static SessionSerializerContext Get(byte v) => v switch
    {
        1 => V1,
        2 => V2,
        _ => throw new ArgumentOutOfRangeException(nameof(v))
    };
}
