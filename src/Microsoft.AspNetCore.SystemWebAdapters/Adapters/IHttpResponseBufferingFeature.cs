// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpResponseBufferingFeature
{
    void EnableBuffering(int memoryThreshold, long? bufferLimit);

    ValueTask FlushAsync();

    [AllowNull]
    Stream Filter { get; set; }

    bool IsEnabled { get; }
}

#endif
