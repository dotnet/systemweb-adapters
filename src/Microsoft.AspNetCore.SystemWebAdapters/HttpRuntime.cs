// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

public sealed class HttpRuntime
{
    static HttpRuntime()
    {
        AppDomainAppVirtualPath = NativeMethods.IsAspNetCoreModuleLoaded()
            ? NativeMethods.HttpGetApplicationProperties().pwzVirtualApplicationPath
            : "/";
    }

    private HttpRuntime()
    {
    }

    public static string AppDomainAppVirtualPath { get; }
}
