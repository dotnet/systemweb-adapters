// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Represents whether a response has ended or allows transition into an ended state.
/// </summary>
#if NET8_0_OR_GREATER
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public
#else
internal
#endif
interface IHttpResponseEndFeature
{
    bool IsEnded { get; }

    Task EndAsync();
}

#endif
