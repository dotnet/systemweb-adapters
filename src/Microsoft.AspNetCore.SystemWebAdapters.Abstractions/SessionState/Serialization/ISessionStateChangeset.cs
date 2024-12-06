// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public interface ISessionStateChangeset : ISessionState
{
    IEnumerable<SessionStateChangeItem> Changes { get; }
}
