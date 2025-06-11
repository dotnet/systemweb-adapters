// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Hosting;

public abstract class HostedHttpApplication : HttpApplication
{
    public static HttpApplicationHost Host => HttpApplicationHost.Current;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Managed by IIS lifetime management")]
    protected virtual void Application_Start()
    {
        var builder = HttpApplicationHostBuilder.Create();
        ConfigureHost(builder);
        builder.InitializeHost();
    }

    protected virtual void ConfigureHost(HttpApplicationHostBuilder builder)
    {
    }
}

