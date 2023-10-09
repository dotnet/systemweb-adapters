// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents any errors raised during <see cref="HttpApplication"/> events.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IRequestExceptionFeature
{
    IReadOnlyList<Exception> Exceptions { get; }

    void Add(Exception exception);

    void Clear();
}

#endif
