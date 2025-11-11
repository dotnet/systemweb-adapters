// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

internal sealed partial class RequestUserFeature : IRequestUserFeature, IHttpAuthenticationFeature, IDisposable
{
    private readonly ILogger _logger;
    private bool _setCurrentAccessors;

    public RequestUserFeature(HttpContextCore context)
    {
        var logger = (ILogger?)context.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger<RequestUserFeature>();

        _logger = logger ?? NullLogger.Instance;

        User = context.User;
    }

    [LoggerMessage(0, LogLevel.Debug, "A custom principal {PrincipalType} is being used and should be replaced with a ClaimsPrincipal derived type.")]
    private partial void LogNonClaimsPrincipal(Type principalType);

    [LoggerMessage(1, LogLevel.Trace, "Thread.CurrentPrincipal has been set with the current user")]
    private partial void LogCurrentPrincipalUsage();

    public IPrincipal? User { get; set; }

    WindowsIdentity? IRequestUserFeature.LogonUserIdentity => User?.Identity as WindowsIdentity;

    ClaimsPrincipal? IHttpAuthenticationFeature.User
    {
        get => GetOrCreateClaims(User);
        set
        {
            User = value;
            EnsureCurrentPrincipalSetIfRequired();
        }
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

    public void EnableStaticAccessors()
    {
        LogCurrentPrincipalUsage();
        _setCurrentAccessors = true;
        EnsureCurrentPrincipalSetIfRequired();
    }

    private void EnsureCurrentPrincipalSetIfRequired()
    {
        if (_setCurrentAccessors)
        {
            var claimsPrincipal = GetOrCreateClaims(User);
            Thread.CurrentPrincipal = claimsPrincipal;
        }
    }

    public void Dispose()
    {
        if (_setCurrentAccessors)
        {
            Thread.CurrentPrincipal = null;
        }
    }
}
