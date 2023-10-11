// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class SessionStateMiddleware
{
    private readonly RequestDelegate _next;

    public SessionStateMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Features.Set<ISessionStateFeature>(new SessionStateFeature(context));
        await _next(context);
        context.Features.Set<ISessionStateFeature>(null);
    }

    private sealed class SessionStateFeature : ISessionStateFeature
    {
        private readonly HttpContext _context;
        private SessionStateBehavior? _behavior;
        private HttpSessionState? _session;

        public SessionStateFeature(HttpContext context)
        {
            _context = context;
        }

        public SessionStateBehavior Behavior
        {
            get => _behavior is { } behavior ? behavior : GetExisting()?.SessionBehavior ?? default;
            set => _behavior = value;
        }

        public HttpSessionState? Session => State is null ? null : _session ??= new(this);

        public ISessionState? State { get; set; }

        public bool IsLazyLoad => GetExisting() is { IsLazyLoad: true };

        private SessionAttribute? GetExisting()
            => _context.GetEndpoint()?.Metadata.GetMetadata<SessionAttribute>();
    }
}
