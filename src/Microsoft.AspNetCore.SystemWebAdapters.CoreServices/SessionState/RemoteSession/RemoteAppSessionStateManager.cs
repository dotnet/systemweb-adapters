// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal partial class RemoteAppSessionStateManager : ISessionManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionSerializer _serializer;
    private readonly ILogger<RemoteAppSessionStateManager> _logger;
    private readonly RemoteAppSessionStateClientOptions _options;
    private readonly IOptions<RemoteAppClientOptions> _remoteAppOptions;

    private HttpClient? _client;

    private HttpClient Client
    {
        get
        {
            if (_client is null)
            {
                // Use the HttpClient supplied in options if one is present in options;
                // otherwise, generate a client with an IHttpClientFactory from DI
                _client = _remoteAppOptions.Value.BackchannelHttpClient ?? _httpClientFactory.CreateClient(SessionConstants.SessionClientName);
                _client.BaseAddress = new Uri($"{_remoteAppOptions.Value.RemoteAppUrl.ToString().TrimEnd('/')}{_options.SessionEndpointPath}");
                _client.DefaultRequestHeaders.Add(_remoteAppOptions.Value.ApiKeyHeader, _remoteAppOptions.Value.ApiKey);
            }

            return _client;
        }
    }

    public RemoteAppSessionStateManager(
        IHttpClientFactory httpClientFactory,
        ISessionSerializer serializer,
        IOptions<RemoteAppSessionStateClientOptions> sessionOptions,
        IOptions<RemoteAppClientOptions> remoteAppOptions,
        ILogger<RemoteAppSessionStateManager> logger)
    {
        _remoteAppOptions = remoteAppOptions ?? throw new ArgumentNullException(nameof(remoteAppOptions));
        _options = sessionOptions?.Value ?? throw new ArgumentNullException(nameof(sessionOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Loaded {Count} items from remote session state for session {SessionId}")]
    private partial void LogSessionLoad(int count, string sessionId);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unable to load remote session state for session {SessionId}")]
    private partial void LogRemoteSessionException(Exception exc, string? sessionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Received {StatusCode} response getting remote session state")]
    private partial void LogRetrieveResponse(HttpStatusCode statusCode);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Received {StatusCode} response committing remote session state")]
    private partial void LogCommitResponse(HttpStatusCode statusCode);

    public async Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
    {
        // If an existing remote session ID is present in the request, use its session ID.
        // Otherwise, leave session ID null for now; it will be provided by the remote service
        // when session data is loaded.
        var sessionId = context.Request.Cookies[_options.CookieName];

        try
        {
            // Get or create session data
            var response = await GetSessionDataAsync(sessionId, metadata.IsReadOnly, context, context.RequestAborted);

            LogSessionLoad(response.Count, response.SessionID);

            return response;
        }
        catch (Exception exc)
        {
            LogRemoteSessionException(exc, sessionId);
            throw;
        }
    }

    private async Task<ISessionState> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken token)
    {
        // The request message is manually disposed at a later time
#pragma warning disable CA2000 // Dispose objects before losing scope
        var req = new HttpRequestMessage { Method = HttpMethod.Get };
#pragma warning restore CA2000 // Dispose objects before losing scope

        AddSessionCookieToHeader(req, sessionId);
        AddReadOnlyHeader(req, readOnly);

        var response = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);

        LogRetrieveResponse(response.StatusCode);

        response.EnsureSuccessStatusCode();

        var remoteSessionState = await _serializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(token), token);

        if (remoteSessionState is null)
        {
            throw new InvalidOperationException("Could not retrieve remote session state");
        }

        // Propagate headers back to the caller since a new session ID may have been set
        // by the remote app if there was no session active previously or if the previous
        // session expired.
        PropagateHeaders(response, callingContext, HeaderNames.SetCookie);

        if (remoteSessionState.IsReadOnly)
        {
            response.Dispose();
            return remoteSessionState;
        }

        return new RemoteSessionState(remoteSessionState, response, SetOrReleaseSessionData);
    }

    /// <summary>
    /// Commits changes to the server. Passing null <paramref name="state"/> will release the session lock but not update session data.
    /// </summary>
    private async Task SetOrReleaseSessionData(ISessionState? state, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage { Method = HttpMethod.Put };

        if (state is not null)
        {
            AddSessionCookieToHeader(req, state.SessionID);
            req.Content = new SerializedSessionHttpContent(_serializer, state);
        }

        using var response = await Client.SendAsync(req, cancellationToken);

        LogCommitResponse(response.StatusCode);

        response.EnsureSuccessStatusCode();
    }

    private static void PropagateHeaders(HttpResponseMessage responseMessage, HttpContextCore context, string cookieName)
    {
        if (context?.Response is not null && responseMessage.Headers.TryGetValues(cookieName, out var cookieValues))
        {
            context.Response.Headers.Add(cookieName, cookieValues.ToArray());
        }
    }

    private void AddSessionCookieToHeader(HttpRequestMessage req, string? sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            req.Headers.Add(HeaderNames.Cookie, $"{_options.CookieName}={sessionId}");
        }
    }

    private static void AddReadOnlyHeader(HttpRequestMessage req, bool readOnly)
        => req.Headers.Add(SessionConstants.ReadOnlyHeaderName, readOnly.ToString());

    private class SerializedSessionHttpContent : HttpContent
    {
        private readonly ISessionSerializer _serializer;
        private readonly ISessionState _state;

        public SerializedSessionHttpContent(ISessionSerializer serializer, ISessionState state)
        {
            _serializer = serializer;
            _state = state;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, default);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
            => _serializer.SerializeAsync(_state, stream, cancellationToken);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
