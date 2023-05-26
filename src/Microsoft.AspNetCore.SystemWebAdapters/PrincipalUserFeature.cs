// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

internal sealed class PrincipalUserFeature : IPrincipalUserFeature, IHttpAuthenticationFeature
{
    private ClaimsPrincipal? _claims;
    private IPrincipal? _principal;

    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => _claims ?? GetClaimsPrincipal(_principal);
        set
        {
            _claims = value;
            _principal = value;
        }
    }

    public IPrincipal? User
    {
        get => _claims ?? _principal;
        set
        {
            _principal = value;
            _claims = null;
        }
    }

    private static ClaimsPrincipal? GetClaimsPrincipal(IPrincipal? principal) => principal switch
    {
        null => null,
        ClaimsPrincipal claims => claims,
        _ => new ClaimsPrincipal(principal),
    };
}
