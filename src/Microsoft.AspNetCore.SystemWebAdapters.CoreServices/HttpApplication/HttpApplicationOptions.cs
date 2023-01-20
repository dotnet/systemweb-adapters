// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpApplicationOptions
{
    internal bool IsHttpApplicationNeeded => Modules.Count > 0 || ApplicationType != typeof(HttpApplication);

    public Type ApplicationType { get; set; } = typeof(HttpApplication);

    public ICollection<Type> Modules { get; } = new List<Type>();

    public int PoolSize { get; set; } = 10;

    internal Func<IServiceProvider, HttpApplication> Factory { get; set; } = null!;

    public void RegisterModule<T>()
         where T : IHttpModule
        => Modules.Add(typeof(T));
}
