// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using System.Web.Hosting;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNet;

[assembly: PreApplicationStartMethod(typeof(SamplesServiceDefaultsStartup), nameof(SamplesServiceDefaultsStartup.Startup))]

namespace System.Web;

public static class SamplesServiceDefaultsStartup
{
    public static void Startup()
    {
        HttpApplication.RegisterModule(typeof(TelemetryHttpModule));
    }
}
