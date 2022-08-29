// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

/// <summary>
/// Represents the state of a session and is used to create a <see cref="HttpSessionState"/>.
/// </summary>
public interface ISessionState : IDisposable
{
    string SessionID { get; }

    bool IsReadOnly { get; }

    int Timeout { get; set; }

    bool IsNewSession { get; }

    int Count { get; }

    bool IsSynchronized { get; }

    object SyncRoot { get; }

    bool IsAbandoned { get; set; }

    object? this[string key] { get; set; }

    void Remove(string key);

    void Clear();

    IEnumerable<string> Keys { get; }

    Task CommitAsync(CancellationToken token);
}
