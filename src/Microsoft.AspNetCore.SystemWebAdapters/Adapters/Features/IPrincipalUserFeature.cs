// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents the user as an <see cref="IPrincipal"/> as opposed to the in-built <see cref="IHttpAuthenticationFeature.User"/> which
/// expects a <see cref="ClaimsPrincipal"/>.
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface IPrincipalUserFeature
{
    IPrincipal? User { get; set; }
}

#endif
