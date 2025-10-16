// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public enum SameSiteMode
{
    None = 0,
    Lax = 1,
    Strict = 2,
}

public sealed partial class HttpRuntime
{
    public static System.IServiceProvider WebObjectActivator { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web"); } }
}
