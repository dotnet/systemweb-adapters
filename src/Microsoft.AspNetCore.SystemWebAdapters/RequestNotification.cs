// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

[Flags]
public enum RequestNotification
{
    BeginRequest = 0x00000001,  // request is beginning
    AuthenticateRequest = 0x00000002,  // request is being authenticated
    AuthorizeRequest = 0x00000004,  // request is being authorized
    ResolveRequestCache = 0x00000008,  // satisfy request from cache
    MapRequestHandler = 0x00000010,  // map handler for request
    AcquireRequestState = 0x00000020,  // acquire request state
    PreExecuteRequestHandler = 0x00000040,
    ExecuteRequestHandler = 0x00000080,  // execute handler
    ReleaseRequestState = 0x00000100,  // release request state
    UpdateRequestCache = 0x00000200,  // update cache
    LogRequest = 0x00000400,  // log request
    EndRequest = 0x00000800,  // end request
    SendResponse = 0x20000000   // send response
}
