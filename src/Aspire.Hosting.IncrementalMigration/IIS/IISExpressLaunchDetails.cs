// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting;

internal sealed record class IISExpressLaunchDetails
{
    public bool Use64BitIISExpress { get; init; } = Environment.Is64BitOperatingSystem;

    public int? SslPort { get; init; }

    public int? HttpPort { get; init; }
}
