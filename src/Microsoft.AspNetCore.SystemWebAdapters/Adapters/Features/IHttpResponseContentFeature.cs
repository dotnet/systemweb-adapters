// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// A feature that allows clearing the content or suppressing it.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IHttpResponseContentFeature
{
    bool SuppressContent { get; set; }

    void ClearContent();
}

#endif
