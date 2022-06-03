// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public interface ISessionSerializer
{
    Task<ISessionState?> DeserializeAsync(Stream stream, CancellationToken token);

    Task SerializeAsync(ISessionState state, Stream stream, CancellationToken token);
}
