// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// This is used to dispatch to either <see cref="DoubleConnectionRemoteAppSessionManager"/> if in read-only mode or <see cref="SingleConnectionWriteableRemoteAppSessionStateManager"/> if not.
/// </summary>
internal sealed class RemoteAppSessionDispatcher : ISessionManager
{
    private readonly IOptions<RemoteAppSessionStateClientOptions> _options;
    private readonly ISessionManager _singleConnection;
    private readonly ISessionManager _doubleConnection;

    private bool _singleConnectionSupported = true;

    public static ISessionManager Create(
        IOptions<RemoteAppSessionStateClientOptions> options,
        ISessionManager singleConnection,
        ISessionManager doubleConnection
        )
    {
        return new RemoteAppSessionDispatcher(options, singleConnection, doubleConnection);
    }

    public RemoteAppSessionDispatcher(
        IOptions<RemoteAppSessionStateClientOptions> options,
        SingleConnectionWriteableRemoteAppSessionStateManager singleConnection,
        DoubleConnectionRemoteAppSessionManager doubleConnection
        )
        : this(options, (ISessionManager)singleConnection, doubleConnection)
    {
    }

    private RemoteAppSessionDispatcher(
        IOptions<RemoteAppSessionStateClientOptions> options,
        ISessionManager singleConnection,
        ISessionManager doubleConnection)
    {
        _options = options;
        _singleConnection = singleConnection;
        _doubleConnection = doubleConnection;
    }

    public async Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        if (metadata.IsReadOnly)
        {
            // In readonly mode it's a simple GET request
            return await _doubleConnection.CreateAsync(context, metadata);
        }

        if (_singleConnectionSupported && _options.Value.UseSingleConnection)
        {
            try
            {
                return await _singleConnection.CreateAsync(context, metadata);
            }

            // We can attempt to discover if the server supports the single connection. If it doesn't,
            // future attempts will fallback to the double and require a restart of the ASP.NET Core applicatoin
            // to start using the single request.
            catch (HttpRequestException ex) when (ServerDoesNotSupportSingleConnection(ex))
            {
                _singleConnectionSupported = false;
            }
        }

        return await _doubleConnection.CreateAsync(context, metadata);
    }

    private static bool ServerDoesNotSupportSingleConnection(HttpRequestException ex)
    {
#if NET8_0_OR_GREATER
        // This is thrown when HTTP2 cannot be initiated
        if (ex.HttpRequestError == HttpRequestError.HttpProtocolError)
        {
            return true;
        }
#endif

        // This is thrown if the server does not know about the POST verb
        return ex.StatusCode == HttpStatusCode.MethodNotAllowed;
    }
}
