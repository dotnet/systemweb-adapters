// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Owin;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class OwinAppOptions
{
    public Action<IAppBuilder, IServiceProvider>? Configure { get; set; }
}
