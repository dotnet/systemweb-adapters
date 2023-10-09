// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Feature to allow buffering the response.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IHttpResponseBufferingFeature
{
    void EnableBuffering(int memoryThreshold, long? bufferLimit);

    ValueTask FlushAsync();

    [AllowNull]
    Stream Filter { get; set; }

    bool IsEnabled { get; }
}

#endif
