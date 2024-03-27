// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the handlers for a given request.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IHttpHandlerFeature
{
    /// <summary>
    /// Gets or sets the current <see cref="IHttpHandler"/>.
    /// </summary>
    IHttpHandler? Current { get; set; }

    /// <summary>
    /// Gets the previous handlers. When <see cref="Current"/> is set, the previous value should be surfaced here.
    /// </summary>
    IHttpHandler? Previous { get; }
}

#endif
