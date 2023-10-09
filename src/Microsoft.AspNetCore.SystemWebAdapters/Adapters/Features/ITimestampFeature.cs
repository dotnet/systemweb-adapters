// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the timestamp of a request.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface ITimestampFeature
{
    DateTimeOffset Timestamp { get; }
}

#endif
