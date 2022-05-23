// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal enum SessionSaveResult
{
    Success,
    DeserializationError,
    AlreadyUpdated,
    SessionNotFound,
}
