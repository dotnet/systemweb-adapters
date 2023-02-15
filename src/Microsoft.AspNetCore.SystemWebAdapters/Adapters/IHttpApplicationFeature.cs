// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// A feature that exposes ability to raise events on the current <see cref="HttpApplication"/>. 
/// See https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication#remarks for details on how this worked in .NET Framework.
/// </summary>
internal interface IHttpApplicationFeature
{
    HttpApplication Application { get; }

    ValueTask RaiseEventAsync(ApplicationEvent @event);

    RequestNotification CurrentNotification { get; }

    bool IsPostNotification { get; }
}

#endif
