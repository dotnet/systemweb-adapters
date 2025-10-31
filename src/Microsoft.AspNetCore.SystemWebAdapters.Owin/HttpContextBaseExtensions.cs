// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Owin;

namespace System.Web;

/// <summary>
/// Provides extension methods for <see cref="HttpContextBase"/>.
/// </summary>
public static partial class HttpContextBaseExtensions
{
    /// <summary>
    /// Gets the <see cref="IOwinContext"/> for the current request.
    /// </summary>
    public static IOwinContext GetOwinContext(this HttpContextBase context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is HttpContextWrapper wrapper)
        {
            return wrapper.InnerContext.GetOwinContext();
        }

        if (context.Items[OwinConstants.OwinEnvironmentKey] is IDictionary<string, object> env)
        {
            return new OwinContext(env);
        }

        throw new InvalidOperationException("The HttpContextBase does not have an associated OWIN context.");
    }
}
