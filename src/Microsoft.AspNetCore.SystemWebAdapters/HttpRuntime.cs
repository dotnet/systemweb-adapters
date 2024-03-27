// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Caching;
using System.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public sealed class HttpRuntime
{
    private HttpRuntime()
    {
    }

    public static string AppDomainAppVirtualPath => HostingEnvironmentAccessor.Current.Options.AppDomainAppVirtualPath;

    public static string AppDomainAppPath => HostingEnvironmentAccessor.Current.Options.AppDomainAppPath;

    public static IServiceProvider WebObjectActivator => HostingEnvironmentAccessor.Current.Services;

    public static Cache Cache => WebObjectActivator.GetRequiredService<Cache>();
}
