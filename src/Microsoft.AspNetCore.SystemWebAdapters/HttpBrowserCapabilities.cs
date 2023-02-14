// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Configuration;

namespace System.Web;

public class HttpBrowserCapabilities : HttpCapabilitiesBase
{
    internal HttpBrowserCapabilities(HttpContextCore core)
        : base(core)
    {
    }
}
