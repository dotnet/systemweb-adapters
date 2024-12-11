// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// This is used to dispatch to either <see cref="DoubleConnectionRemoteAppSessionManager"/> if in read-only mode or <see cref="SingleConnectionWriteableRemoteAppSessionStateManager"/> if not.
/// </summary>
internal sealed class RemoteAppSessionDispatcher(
    IOptions<RemoteAppSessionStateClientOptions> options,
    SingleConnectionWriteableRemoteAppSessionStateManager singleConnection,
    DoubleConnectionRemoteAppSessionManager doubleConnection
    ) : ISessionManager
{
    private bool _singleConnectionSupported = true;

    public async Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        if (metadata.IsReadOnly)
        {
            // In readonly mode it's a simple GET request
            return await doubleConnection.GetReadOnlySessionStateAsync(context);
        }

        if (!_singleConnectionSupported || options.Value.UseSingleConnection)
        {
            return await doubleConnection.CreateAsync(context, metadata);
        }

        try
        {
            return await singleConnection.CreateAsync(context, metadata);
        }

        // If 
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.MethodNotAllowed)
        {
            _singleConnectionSupported = false;
            return await doubleConnection.CreateAsync(context, metadata);
        }
    }
}
