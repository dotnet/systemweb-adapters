// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the users that ASP.NET Framework used.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IRequestUserFeature
{
    /// <summary>
    /// Gets or sets the user that corresponds to <see cref="HttpContext.User" />
    /// </summary>
    IPrincipal? User { get; set; }

    /// <summary>
    /// Gets the logged on user that corresponds to <see cref="HttpRequest.LogonUserIdentity" />
    /// </summary>
    WindowsIdentity? LogonUserIdentity { get; }

    /// <summary>
    /// Enables access to <see cref="Thread.CurrentPrincipal"/> and <see cref="ClaimsPrincipal.Current"/> for the duration of the request.
    /// </summary>
    void EnableStaticAccessors();
}

#endif
