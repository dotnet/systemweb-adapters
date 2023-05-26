// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

internal sealed class PrincipalUserFeature : IPrincipalUserFeature, IHttpAuthenticationFeature
{
    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => GetClaimsPrincipal(User);
        set => User = value;
    }

    public IPrincipal? User { get; set; }

    private static ClaimsPrincipal? GetClaimsPrincipal(IPrincipal? principal) => principal switch
    {
        null => null,
        ClaimsPrincipal claims => claims,
        _ => new ClaimsPrincipal(principal),
    };
}
