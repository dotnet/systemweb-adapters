// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal static partial class SessionStateExtensions
{
    [LoggerMessage(0, LogLevel.Warning, "Unknown session key '{KeyName}' was received.")]
    private static partial void LogUnknownSessionKey(ILogger logger, string keyName);

    public static void CopyTo(this ISessionState result, ILogger logger, HttpSessionStateBase state)
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

        if (result is ISessionStateChangeset changes)
        {
            UpdateFromChanges(changes, logger, state);
        }
        else
        {
            Replace(result, state);
        }
    }

    private static void UpdateFromChanges(ISessionStateChangeset from, ILogger logger, HttpSessionStateBase state)
    {
        foreach (var change in from.Changes)
        {
            if (change.State is SessionItemChangeState.Changed or SessionItemChangeState.New)
            {
                state[change.Key] = from[change.Key];
            }
            else if (change.State is SessionItemChangeState.Removed)
            {
                state.Remove(change.Key);
            }
            else if (change.State is SessionItemChangeState.Unknown)
            {
                LogUnknownSessionKey(logger, change.Key);
            }
        }
    }

    private static void Replace(ISessionState from, HttpSessionStateBase state)
    {
        state.Clear();

        foreach (var key in from.Keys)
        {
            state[key] = from[key];
        }
    }
}
