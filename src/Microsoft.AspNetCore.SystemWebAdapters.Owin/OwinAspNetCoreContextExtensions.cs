// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public static class OwinAspNetCoreContextExtensions
{
    /// <summary>
    /// Gets the <see cref="HttpContext"/> from an <see cref="IOwinContext"/>."/>
    /// </summary>
    public static HttpContext GetHttpContext(this IOwinContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        return ctx.Get<HttpContext>(typeof(HttpContext).FullName) ?? throw new InvalidOperationException("Could not get an HttpContext");
    }

    /// <summary>
    /// Gets an <see cref="IOwinContext"/> from the given <see cref="HttpContext"/>."/>
    /// </summary>
    public static IOwinContext GetOwinContext(this HttpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        return new OwinContext(ctx.GetOrCreateOwinEnvironment());
    }

    private static IDictionary<string, object> GetOrCreateOwinEnvironment(this HttpContext context)
    {
        var owinEnvFeature = context.Features.Get<IOwinEnvironmentFeature>();

        if (owinEnvFeature is null)
        {
            owinEnvFeature = new OwinEnvironmentFeature() { Environment = new OwinEnvironment(context) };
            context.Features.Set<IOwinEnvironmentFeature>(owinEnvFeature);
        }

        owinEnvFeature.Environment[OwinConstants.OwinEnvironmentKey] = owinEnvFeature.Environment;
        owinEnvFeature.Environment[typeof(HttpContext).FullName] = context;

        return owinEnvFeature.Environment;
    }
}
