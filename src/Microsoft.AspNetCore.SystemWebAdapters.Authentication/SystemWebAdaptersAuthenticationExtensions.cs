// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public static class SystemWebAdaptersExtensions
{
    public static IApplicationBuilder UseClaimsPrincipalForwarding(this IApplicationBuilder app) =>
        app.UseMiddleware<ClaimsPrincipalForwardingMiddleware>();
}
