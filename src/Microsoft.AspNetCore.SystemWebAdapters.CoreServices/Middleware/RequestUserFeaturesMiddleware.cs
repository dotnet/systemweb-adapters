// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Middleware;

internal sealed partial class RequestUserFeaturesMiddleware(RequestDelegate next, IOptions<SystemWebAdaptersOptions> options, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestUserFeature>();
    private readonly bool _setCurrentAccessors = options.Value.EnableStaticUserAccessors;

    public async Task InvokeAsync(HttpContextCore context)
    {
        using var userFeature = new RequestUserFeature(_logger, _setCurrentAccessors) { User = context.User };

        context.Features.Set<IRequestUserFeature>(userFeature);
        context.Features.Set<IHttpAuthenticationFeature>(userFeature);

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set<IRequestUserFeature>(null);
            context.Features.Set<IHttpAuthenticationFeature>(null);
        }
    }

    private sealed partial class RequestUserFeature(ILogger logger, bool setCurrentAccessors) : IRequestUserFeature, IHttpAuthenticationFeature, IDisposable
    {
        private readonly ILogger logger = logger;

        [LoggerMessage(0, LogLevel.Debug, "A custom principal {PrincipalType} is being used and should be replaced with a ClaimsPrincipal derived type.")]
        private partial void LogNonClaimsPrincipal(Type principalType);

        [LoggerMessage(1, LogLevel.Trace, "Thread.CurrentPrincipal has been set with the current user")]
        private partial void LogCurrentPrincipalUsage();

        public IPrincipal? User
        {
            get;
            set
            {
                field = value;
                EnsureCurrentPrincipalSetIfRequired();
            }
        }

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

        void IRequestUserFeature.EnableStaticAccessors()
        {
            LogCurrentPrincipalUsage();
            setCurrentAccessors = true;
            EnsureCurrentPrincipalSetIfRequired();
        }

        private void EnsureCurrentPrincipalSetIfRequired()
        {
            if (setCurrentAccessors)
            {
                var claimsPrincipal = GetOrCreateClaims(User);
                Thread.CurrentPrincipal = claimsPrincipal;
            }
        }

        public void Dispose()
        {
            if (setCurrentAccessors)
            {
                Thread.CurrentPrincipal = null;
            }
        }
    }
}
