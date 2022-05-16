// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class ClaimsPrincipalForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsPrincipalForwardingMiddleware> _logger;

    public ClaimsPrincipalForwardingMiddleware(RequestDelegate next, ILogger<ClaimsPrincipalForwardingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.User is not null)
        {
            var serializedPrincipal = SerializeClaimsPrincipal(context.User);
            AddIdentityVariable(context.Features.Get<IServerVariablesFeature>(), serializedPrincipal);
            AddIdentityHeader(context.Request, serializedPrincipal);
        }

        return _next(context);
    }

    private string SerializeClaimsPrincipal(ClaimsPrincipal principal)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        principal.WriteTo(writer);
        writer.Flush();

        return Convert.ToBase64String(ms.ToArray());
    }

    private void AddIdentityHeader(HttpRequest request, string serializedPrincipal)
    {
        request.Headers.Add(Constants.IdentityVariableName, serializedPrincipal);
        _logger.LogDebug("Stored claims principal in identity header");
    }

    private void AddIdentityVariable(IServerVariablesFeature? serverVariablesFeature, string serializedPrincipal)
    {
        if (serverVariablesFeature is null)
        {
            _logger.LogError("Server variables not available; cannot forward identity");
            return;
        }

        serverVariablesFeature[Constants.IdentityVariableName] = serializedPrincipal;
        _logger.LogDebug("Stored claims principal in identity server variable");
    }
}
