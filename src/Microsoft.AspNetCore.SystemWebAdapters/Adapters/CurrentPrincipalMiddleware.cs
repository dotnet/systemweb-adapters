// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static partial class CurrentPrincipalExtensions
{
    [LoggerMessage(0, LogLevel.Warning, "ClaimsPrincipal.Current was accessed")]
    private static partial void LogClaimsPrincipalAccess(ILogger logger);

    [LoggerMessage(1, LogLevel.Warning, "ClaimsPrincipal.Current will only be set if ISetThreadCurrentPrincipal is set on the endpoint")]
    private static partial void LogClaimsPrincipalAccessNoEndpoint(ILogger logger);

    public static void UseCurrentPrincipal(this IApplicationBuilder app)
    {
        app.UseMiddleware<CurrentPrincipalMiddleware>();

        var accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<ClaimsPrincipal>>();

        ClaimsPrincipal.ClaimsPrincipalSelector = ClaimsPrincipleAccessor!;

        ClaimsPrincipal? ClaimsPrincipleAccessor()
        {
            var context = accessor.HttpContext;

            if (context?.GetEndpoint()?.Metadata.GetMetadata<ISetThreadCurrentPrincipal>() is { IsEnabled: true })
            {
                LogClaimsPrincipalAccess(logger);
                return accessor.HttpContext?.User;
            }

            LogClaimsPrincipalAccessNoEndpoint(logger);
            return null;
        }
    }

    public static IPrincipal WrapUserWithWarning(this HttpContext context)
        => new PrincipalAccessor(context.User, context.RequestServices.GetRequiredService<ILogger<PrincipalAccessor>>());

    private partial class PrincipalAccessor : IPrincipal
    {
        [LoggerMessage(0, LogLevel.Warning, "Thread.Current was accessed via {Method}")]
        private partial void LogAccess([CallerMemberName] string? method = null);

        private readonly IPrincipal _other;
        private readonly ILogger _logger;

        public PrincipalAccessor(IPrincipal other, ILogger logger)
        {
            _other = other;
            _logger = logger;
        }

        public IIdentity? Identity
        {
            get
            {
                LogAccess();
                return _other.Identity;
            }
        }

        public bool IsInRole(string role)
        {
            LogAccess();
            return _other.IsInRole(role);
        }
    }
}

internal class CurrentPrincipalMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentPrincipalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<ISetThreadCurrentPrincipal>() is { IsEnabled: true } ? SetUserAsync(context) : _next(context);

    private async Task SetUserAsync(HttpContext context)
    {
        var current = Thread.CurrentPrincipal;

        try
        {
            Thread.CurrentPrincipal = context.WrapUserWithWarning();

            await _next(context);
        }
        finally
        {
            Thread.CurrentPrincipal = current;
        }
    }
}
