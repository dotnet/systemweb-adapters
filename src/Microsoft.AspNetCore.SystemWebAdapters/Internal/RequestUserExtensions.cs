// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static partial class RequestUserExtensions
{
    public static IRequestUserFeature GetRequestUser(this HttpContextCore context)
    {
        if (context.Features.Get<IRequestUserFeature>() is { } existing)
        {
            return existing;
        }

        var newFeature = new RequestUserFeature(context) { User = context.User };

        context.Features.Set<IRequestUserFeature>(newFeature);
        context.Features.Set<IHttpAuthenticationFeature>(newFeature);

        return newFeature;
    }

    private sealed partial class RequestUserFeature : IRequestUserFeature, IHttpAuthenticationFeature
    {
        private readonly ILogger _logger;

        public RequestUserFeature(HttpContextCore context)
        {
            var logger = (ILogger?)context.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger<RequestUserFeature>();

            _logger = logger ?? NullLogger.Instance;

            User = context.User;
        }

        [LoggerMessage(0, LogLevel.Debug, "A custom principal {PrincipalType} is being used and should be replaced with a ClaimsPrincipal derived type.")]
        private partial void LogNonClaimsPrincipal(Type principalType);

        public IPrincipal? User { get; set; }

        WindowsIdentity? IRequestUserFeature.LogonUserIdentity => User?.Identity as WindowsIdentity;

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
}
