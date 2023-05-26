// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

internal sealed class PrincipalUserFeature : IPrincipalUserFeature, IHttpAuthenticationFeature
{
    public IPrincipal? User { get; set; }

    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => User switch
        {
            null => null,
            ClaimsPrincipal claims => claims,
            IPrincipal user => new ClaimsPrincipal(user),
        };
        set => User = value;
    }
}
