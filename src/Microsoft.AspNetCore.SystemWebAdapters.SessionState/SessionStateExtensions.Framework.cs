// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal static class SessionStateExtensions
{
    public static void CopyTo(this ISessionState result, HttpSessionStateBase state)
    {
        if (!string.Equals(state.SessionID, result.SessionID, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Session id must match");
        }

        if (result.IsAbandoned)
        {
            state.Abandon();
            return;
        }

        state.Timeout = result.Timeout;
        state.Clear();

        foreach (var key in result.Keys)
        {
            state[key] = result[key];
        }
    }
}
