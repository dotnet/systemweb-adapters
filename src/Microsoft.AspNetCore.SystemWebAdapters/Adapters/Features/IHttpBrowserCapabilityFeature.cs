// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using System.Diagnostics.CodeAnalysis;
using System.Web;

/// <summary>
/// Represents the key-value pair used by <see cref="HttpRequest.Browser"/>.
/// </summary>
#if NET8_0_OR_GREATER
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public
#else
internal
#endif
interface IHttpBrowserCapabilityFeature
{
    string? this[string key] { get; }
}
#endif
