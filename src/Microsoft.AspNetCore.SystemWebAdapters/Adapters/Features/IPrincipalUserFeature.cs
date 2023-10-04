// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

using Microsoft.AspNetCore.Http.Features.Authentication;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

/// <summary>
/// Represents the user as an <see cref="IPrincipal"/> as opposed to the in-built <see cref="IHttpAuthenticationFeature.User"/> which
/// expects a <see cref="ClaimsPrincipal"/>.
/// </summary>
#if NET8_0_OR_GREATER
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public
#else
internal
#endif
interface IPrincipalUserFeature
{
    IPrincipal? User { get; set; }
}

#endif
