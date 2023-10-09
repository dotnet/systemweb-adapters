// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;

/// <summary>
/// Represents the timestamp of a request.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface ITimestampFeature
{
    DateTimeOffset Timestamp { get; }
}
#endif
