// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal class CurrentPrincipalMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentPrincipalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
        => context.User is { } ? SetUserAsync(context) : _next(context);

    private async Task SetUserAsync(HttpContext context)
    {
        var current = Thread.CurrentPrincipal;

        try
        {
            Thread.CurrentPrincipal = new PrincipalAccessor(context.User, context.RequestServices.GetRequiredService<ILogger<PrincipalAccessor>>());

            await _next(context);
        }
        finally
        {
            Thread.CurrentPrincipal = current;
        }
    }

    private partial class PrincipalAccessor : IPrincipal
    {
        [LoggerMessage(0, LogLevel.Warning, "Thread.Current was accessed via {Method}")]
        static partial void LogAccess([CallerMemberName] string? method = null);

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
