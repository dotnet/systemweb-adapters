// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Features;

[Experimental(Constants.ExperimentalFeatures.DiagnosticId)]
public enum ApplicationEvent
{
    ApplicationStart,

    ApplicationInit,

    BeginRequest,

    AuthenticateRequest,

    PostAuthenticateRequest,

    AuthorizeRequest,

    PostAuthorizeRequest,

    ResolveRequestCache,

    PostResolveRequestCache,

    MapRequestHandler,

    PostMapRequestHandler,

    AcquireRequestState,

    PostAcquireRequestState,

    PreRequestHandlerExecute,

    PostRequestHandlerExecute,

    ReleaseRequestState,

    PostReleaseRequestState,

    UpdateRequestCache,

    PostUpdateRequestCache,

    LogRequest,

    PostLogRequest,

    EndRequest,

    PreSendRequestHeaders,

    Error,

    SessionStart,

    SessionEnd,

    Disposed,
}

#endif
