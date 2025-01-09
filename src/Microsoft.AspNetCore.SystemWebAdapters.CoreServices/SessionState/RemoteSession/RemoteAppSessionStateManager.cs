// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal abstract partial class RemoteAppSessionStateManager : ISessionManager
{
    private readonly ILogger _logger;

    protected RemoteAppSessionStateManager(
        ISessionSerializer serializer,
        IOptions<RemoteAppSessionStateClientOptions> options,
        IOptions<RemoteAppClientOptions> remoteAppClientOptions,
        ILogger logger)
    {
        Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        BackchannelClient = remoteAppClientOptions?.Value.BackchannelClient ?? throw new ArgumentNullException(nameof(remoteAppClientOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    protected ISessionSerializer Serializer { get; }

    protected RemoteAppSessionStateClientOptions Options { get; }

    protected HttpClient BackchannelClient { get; }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Loaded {Count} items from remote session state for session {SessionId}")]
    protected partial void LogSessionLoad(int count, string sessionId);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unable to load remote session state for session {SessionId}")]
    protected partial void LogRemoteSessionException(Exception exc, string? sessionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Received {StatusCode} response getting remote session state")]
    protected partial void LogRetrieveResponse(HttpStatusCode statusCode);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Received {StatusCode} response committing remote session state")]
    protected partial void LogCommitResponse(HttpStatusCode statusCode);

    [LoggerMessage(EventId = 4, Level = LogLevel.Trace, Message = "Server supports version {Version} for serializing")]
    protected partial void LogServerVersionSupport(byte version);

    public Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
        => CreateAsync(context, metadata.IsReadOnly);

    protected async Task<ISessionState> CreateAsync(HttpContextCore context, bool isReadOnly)
    {
        // If an existing remote session ID is present in the request, use its session ID.
        // Otherwise, leave session ID null for now; it will be provided by the remote service
        // when session data is loaded.
        var sessionId = context.Request.Cookies[Options.CookieName];

        try
        {
            // Get or create session data
            var response = await GetSessionDataAsync(sessionId, isReadOnly, context, context.RequestAborted);

            LogSessionLoad(response.Count, response.SessionID);

            return response;
        }
        catch (Exception exc)
        {
            LogRemoteSessionException(exc, sessionId);
            throw;
        }
    }

    protected abstract Task<ISessionState> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken token);

    protected static void PropagateHeaders(HttpResponseMessage responseMessage, HttpContextCore context, string cookieName)
    {
        if (context?.Response is not null && responseMessage.Headers.TryGetValues(cookieName, out var cookieValues))
        {
            context.Response.Headers.AppendList(cookieName, cookieValues.ToArray());
        }
    }

    protected void AddSessionCookieToHeader(HttpRequestMessage req, string? sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            req.Headers.Add(HeaderNames.Cookie, $"{Options.CookieName}={sessionId}");
        }
    }

    protected static void AddRemoteSessionHeaders(HttpRequestMessage req, bool readOnly)
    {
        if (readOnly)
        {
            req.Headers.Add(SessionConstants.ReadOnlyHeaderName, readOnly.ToString());
        }

        req.Headers.Add(SessionConstants.SupportedVersion, SessionSerializerContext.Latest.SupportedVersion.ToString(CultureInfo.InvariantCulture));
    }

    protected SessionSerializerContext GetSupportedSerializerContext(HttpResponseMessage message)
    {
        var context = message.Headers.TryGetValues(SessionConstants.SupportedVersion, out var versions)
            ? SessionSerializerContext.Parse(versions)
            : SessionSerializerContext.Default;

        LogServerVersionSupport(context.SupportedVersion);

        return context;
    }

    protected abstract class RemoteSessionHttpContent : HttpContent
    {
        protected sealed override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected sealed override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, default);
    }
}
