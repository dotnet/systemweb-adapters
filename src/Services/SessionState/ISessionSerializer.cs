// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal interface ISessionSerializer
{
    /// <summary>
    /// Deserializes a session state.
    /// </summary>
    /// <param name="stream">The serialized session stream.</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>If the stream defines a serialized session changeset, it will also implement <see cref="ISessionStateChangeset"/>.</returns>
    Task<ISessionState?> DeserializeAsync(Stream stream, CancellationToken token);

    /// <summary>
    /// Serializes the session state. If the <paramref name="state"/> implements <see cref="ISessionStateChangeset"/> it will serialize it
    /// in a mode that tracks the changes that have occurred.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="stream"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task SerializeAsync(ISessionState state, SessionSerializerContext context, Stream stream, CancellationToken token);
}
