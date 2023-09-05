// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

internal interface ISessionStateFeature
{
    SessionStateBehavior State { get; set; }
}
#endif
