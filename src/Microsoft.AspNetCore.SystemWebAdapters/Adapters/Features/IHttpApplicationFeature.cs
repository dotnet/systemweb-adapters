// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the emulated IIS pipeline and the <see cref="HttpApplication"/> associated with it.
/// </summary>
#if NET8_0_OR_GREATER
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public
#else
internal
#endif
    interface IHttpApplicationFeature
{
    /// <summary>
    /// Gets the <see cref="HttpApplication"/> that is assigned to the current request.
    /// </summary>
    HttpApplication Application { get; }

    /// <summary>
    /// Raises events for the current application assigned to the request. See https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication#remarks for details on how this worked in .NET Framework.
    /// </summary>
    /// <param name="appEvent"></param>
    /// <returns></returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "May need to support async calls")]
    ValueTask RaiseEventAsync(ApplicationEvent appEvent);

    /// <summary>
    /// Gets the current <see cref="RequestNotification"/> of where the request is in an emulated IIS pipeline.
    /// </summary>
    RequestNotification CurrentNotification { get; }

    /// <summary>
    /// Gets whether the <see cref="CurrentNotification"/> of the emulated IIS pipeline is in a post condition.
    /// </summary>
    bool IsPostNotification { get; }
}

#endif
