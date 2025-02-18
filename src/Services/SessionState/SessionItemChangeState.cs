// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal enum SessionItemChangeState
{
    Unknown = 0,
    NoChange = 1,
    Removed = 2,
    Changed = 3,
    New = 4,
}
