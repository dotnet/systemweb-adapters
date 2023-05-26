// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed partial class PrincipalUserFeature : IPrincipalUserFeature, IHttpAuthenticationFeature
{
    private readonly ILogger _logger;

    public PrincipalUserFeature(HttpContextCore context)
    {
        var logger = (ILogger?)context.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger<PrincipalUserFeature>();

        _logger = logger ?? NullLogger.Instance;
    }

    [LoggerMessage(0, LogLevel.Debug, "A custom principal {PrincipalType} is being used and should be replaced with a ClaimsPrincipal derived type.")]
    private partial void LogNonClaimsPrincipal(Type principalType);

    public IPrincipal? User { get; set; }

    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => GetOrCreateClaims(User);
        set => User = value;
    }

    private ClaimsPrincipal? GetOrCreateClaims(IPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        if (principal is ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal;
        }

        LogNonClaimsPrincipal(principal.GetType());

        return new ClaimsPrincipal(principal);
    }
}
