// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Feature to allow buffering the response.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IHttpResponseBufferingFeature
{
    void EnableBuffering(int? memoryThreshold = default, long? bufferLimit = default);

    ValueTask FlushAsync();

    [AllowNull]
    Stream Filter { get; set; }

    void DisableBuffering();

    bool IsEnabled { get; }
}

#endif
