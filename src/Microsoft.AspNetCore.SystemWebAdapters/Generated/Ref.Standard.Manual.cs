// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public enum SameSiteMode
{
    None = 0,
    Lax = 1,
    Strict = 2,
}

public static partial class VirtualPathUtility
{
    // This method is manually defined as it has a nullability attribute defined which the script doesn't strip out for netstandard2.0
    public static string AppendTrailingSlash(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web"); }
}
