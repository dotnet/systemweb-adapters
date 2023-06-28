// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IHttpApplicationFeature
{
    /// <summary>
    /// Gets the <see cref="HttpApplication"/> that is assigned to the current request.
    /// </summary>
    HttpApplication Application { get; }

    /// <summary>
    /// Raises events for the current application assigned to the request. See https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication#remarks for details on how this worked in .NET Framework.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    ValueTask RaiseEventAsync(ApplicationEvent @event);

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
