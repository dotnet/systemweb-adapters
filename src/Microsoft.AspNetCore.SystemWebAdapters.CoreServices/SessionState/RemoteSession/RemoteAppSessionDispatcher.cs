// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// This is used to dispatch to either <see cref="DoubleConnectionRemoteAppSessionManager"/> if in read-only mode or <see cref="SingleConnectionWriteableRemoteAppSessionStateManager"/> if not.
/// </summary>
internal sealed partial class RemoteAppSessionDispatcher : ISessionManager
{
    private readonly IOptions<RemoteAppSessionStateClientOptions> _options;
    private readonly ISessionManager _singleConnection;
    private readonly ISessionManager _doubleConnection;
    private readonly ILogger _logger;

    /// <summary>
    /// This method is used to test the behavior of this class since it can take any ISessionManage instances. Once we drop support for .NET 6, we should be able to use keyed services
    /// in the DI container to achieve the same effect with just a constructor.
    /// </summary>
    public static ISessionManager Create(
        IOptions<RemoteAppSessionStateClientOptions> options,
        ISessionManager singleConnection,
        ISessionManager doubleConnection,
        ILogger logger
        )
    {
        return new RemoteAppSessionDispatcher(options, singleConnection, doubleConnection, logger);
    }

    public RemoteAppSessionDispatcher(
        IOptions<RemoteAppSessionStateClientOptions> options,
        SingleConnectionWriteableRemoteAppSessionStateManager singleConnection,
        DoubleConnectionRemoteAppSessionManager doubleConnection,
        ILogger<RemoteAppSessionDispatcher> logger
        )
        : this(options, (ISessionManager)singleConnection, doubleConnection, logger)
    {
    }

    private RemoteAppSessionDispatcher(
        IOptions<RemoteAppSessionStateClientOptions> options,
        ISessionManager singleConnection,
        ISessionManager doubleConnection,
        ILogger logger)
    {
        _options = options;
        _singleConnection = singleConnection;
        _doubleConnection = doubleConnection;
        _logger = logger;
    }

    public async Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        if (metadata.IsReadOnly)
        {
            // In readonly mode it's a simple GET request
            return await _doubleConnection.CreateAsync(context, metadata);
        }

        if (_options.Value.UseSingleConnection)
        {
            try
            {
                return await _singleConnection.CreateAsync(context, metadata);
            }

            // We can attempt to discover if the server supports the single connection. If it doesn't,
            // future attempts will fallback to the double until the option value is reset.
            catch (HttpRequestException ex) when (ServerDoesNotSupportSingleConnection(ex))
            {
                LogServerDoesNotSupportSingleConnection();
                _options.Value.UseSingleConnection = false;
            }
            catch (Exception ex)
            {
                LogServerFailedSingelConnection(ex);
                throw;
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

    [LoggerMessage(0, LogLevel.Warning, "The server does not support the single connection mode for remote session. Falling back to double connection mode. This must be manually reset to try again.")]
    private partial void LogServerDoesNotSupportSingleConnection();

    [LoggerMessage(1, LogLevel.Error, "Failed to connect to server with an unknown reason")]
    private partial void LogServerFailedSingelConnection(Exception ex);
}
