// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

public sealed class HttpRuntime
{
    private static IHttpRuntime? _current;

    /// <summary>
    /// Gets the current <see cref="IHttpRuntime"/>. This should not be used internally besides where is strictly necessary.
    /// If this is needed, it should be retrieved through dependency injection.
    /// </summary>
    internal static IHttpRuntime Current
    {
        get => _current ?? throw new InvalidOperationException("HttpRuntime is not available in the current environment");
        set => _current = value;
    }

    private HttpRuntime()
    {
    }

    public static string AppDomainAppVirtualPath => Current.AppDomainAppVirtualPath;

    public static string AppDomainAppPath => Current.AppDomainAppPath;

    public static Caching.Cache Cache => Current.Cache;
}
