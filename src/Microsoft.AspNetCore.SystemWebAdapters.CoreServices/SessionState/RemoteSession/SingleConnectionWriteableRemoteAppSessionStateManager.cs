// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// An implementation of <see cref="ISessionManager"/> that uses HTTP2 streaming to POST to session endpoint with a single connection.
/// </summary>
/// <remarks>
/// This only supports non-readonly mode. For readonly mode, <see cref="DoubleConnectionRemoteAppSessionManager"/> should be used. An additional implementation
/// of <see cref="ISessionManager"/> is available that handles the dispatching to the correct implementation. See <see cref="RemoteAppSessionDispatcher"/> for that.
/// </remarks>
internal sealed partial class SingleConnectionWriteableRemoteAppSessionStateManager(
    ISessionSerializer serializer,
    IOptions<RemoteAppSessionStateClientOptions> options,
    IOptions<RemoteAppClientOptions> remoteAppClientOptions,
    ILogger<SingleConnectionWriteableRemoteAppSessionStateManager> logger
    ) : RemoteAppSessionStateManager(serializer, options, remoteAppClientOptions, logger)
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is handled in the returned ISessionState")]
    protected override async Task<ISessionState> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken token)
    {
        if (readOnly)
        {
            throw new InvalidOperationException("This manager is only intended for writeable session");
        }

        var content = new CommittingSessionHttpContent(Serializer);

        // The request message is manually disposed at a later time
        var request = new HttpRequestMessage(HttpMethod.Post, Options.Path.Relative)
        {
            Content = content,
            Version = new Version(2, 0),
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
        };

        AddSessionCookieToHeader(request, sessionId);
        AddRemoteSessionHeaders(request, readOnly);

        var response = await BackchannelClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        LogRetrieveResponse(response.StatusCode);

        response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync(token);
        var remoteSessionState = await Serializer.DeserializeAsync(responseStream, token);

        if (remoteSessionState is null)
        {
            if (responseStream is { })
            {
                await responseStream.DisposeAsync();
            }

            response.Dispose();
            request.Dispose();
            content.Dispose();

            throw new InvalidOperationException("Could not retrieve remote session state");
        }

        // Propagate headers back to the caller since a new session ID may have been set
        // by the remote app if there was no session active previously or if the previous
        // session expired.
        PropagateHeaders(response, callingContext, HeaderNames.SetCookie);

        return new RemoteSessionState(remoteSessionState, request, response, GetSupportedSerializerContext(response), content, responseStream);
    }

    [JsonSerializable(typeof(SessionPostResult))]
    private sealed partial class SessionPostResultContext : JsonSerializerContext
    {
    }

    private sealed class CommittingSessionHttpContent : RemoteSessionHttpContent
    {
        private readonly TaskCompletionSource<(ISessionState, SessionSerializerContext)> _state;

        public CommittingSessionHttpContent(ISessionSerializer serializer)
        {
            Serializer = serializer;
            _state = new();
        }

        public ISessionSerializer Serializer { get; }

        public void Commit(SessionSerializerContext context, ISessionState state) => _state.SetResult((state, context));

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            await stream.FlushAsync(cancellationToken);
            var (state, sessionContext) = await _state.Task;

            await Serializer.SerializeAsync(state, sessionContext, stream, cancellationToken);
        }
    }


    private sealed class RemoteSessionState(ISessionState other, HttpRequestMessage request, HttpResponseMessage response, SessionSerializerContext sessionContext, CommittingSessionHttpContent content, Stream stream) : DelegatingSessionState
    {
        protected override ISessionState State => other;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                stream.Dispose();
                response.Dispose();
                content.Dispose();
                request.Dispose();
            }
        }

        public override async Task CommitAsync(CancellationToken token)
        {
            content.Commit(sessionContext, State);

            var result = await JsonSerializer.DeserializeAsync(stream, SessionPostResultContext.Default.SessionPostResult, token);

            if (result is not { Success: true })
            {
                throw new InvalidOperationException("Failed to commit session state");
            }
        }
    }
}
