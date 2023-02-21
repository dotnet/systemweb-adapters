// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal enum ApplicationEvent
{
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

    RequestCompleted,

    SessionStart,

    SessionEnd,

    Disposed,
}

#endif
