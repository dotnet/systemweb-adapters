// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Security.Principal;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IPrincipalUserFeature
{
    IPrincipal? User { get; set; }
}

#endif
