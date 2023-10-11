// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

/// <summary>
/// Represents session state for System.Web
/// </summary>
[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public interface ISessionStateFeature
{
    SessionStateBehavior Behavior { get; set; }

    bool IsPreLoad { get; }

    HttpSessionState? Session { get; }

    ISessionState? State { get; set; }
}
#endif
