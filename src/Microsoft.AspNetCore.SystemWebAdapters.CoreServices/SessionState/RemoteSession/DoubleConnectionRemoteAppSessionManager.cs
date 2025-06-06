// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

/// <summary>
/// This is an implementation of <see cref="ISessionManager"/> that connects to an upstream session store.
/// <list type="bullet">
/// <item>If the request is readonly, it will close the remote connection and return the session state.</item>
/// <item>If the request is not readonly, it will hold onto the remote connection and initiate a new connection to PUT the results.</item>
/// </list>
/// </summary>
/// <remarks>
/// For the non-readonly mode, it is preferrable to use <see cref="SingleConnectionWriteableRemoteAppSessionStateManager"/> instead
/// which will only use a single connection via HTTP2 streaming.
/// </remarks>
internal sealed class DoubleConnectionRemoteAppSessionManager(
    ISessionSerializer serializer,
    IOptions<RemoteAppSessionStateClientOptions> options,
    IOptions<RemoteAppClientOptions> remoteAppClientOptions,
    ILogger<DoubleConnectionRemoteAppSessionManager> logger
    ) : RemoteAppSessionStateManager(serializer, options, remoteAppClientOptions, logger)
{
    public Task<ISessionState> GetReadOnlySessionStateAsync(HttpContextCore context) => CreateAsync(context, isReadOnly: true);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "They are either passed into another object or are manually disposed")]
    protected override async Task<ISessionState> GetSessionDataAsync(string? sessionId, bool readOnly, HttpContextCore callingContext, CancellationToken token)
    {
        // The request message is manually disposed at a later time
        var request = new HttpRequestMessage(HttpMethod.Get, Options.Path.Relative);

        AddSessionCookieToHeader(request, sessionId);
        AddRemoteSessionHeaders(request, readOnly);

        var response = await BackchannelClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        LogRetrieveResponse(response.StatusCode);

        response.EnsureSuccessStatusCode();

        var remoteSessionState = await Serializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(token), token);

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
            request.Dispose();
            response.Dispose();
            return remoteSessionState;
        }

        return new RemoteSessionState(remoteSessionState, request, response, this);
    }

    private sealed class SerializedSessionHttpContent(
        ISessionSerializer serializer,
        ISessionState state,
        SessionSerializerContext context
        ) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, default);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? _, CancellationToken cancellationToken)
        {
            return serializer.SerializeAsync(state, context, stream, cancellationToken);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class RemoteSessionState(
        ISessionState other,
        HttpRequestMessage request,
        HttpResponseMessage response,
        DoubleConnectionRemoteAppSessionManager manager
        ) : DelegatingSessionState
    {
        protected override ISessionState State => other;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                request.Dispose();
                response.Dispose();
            }
        }

        public override async Task CommitAsync(CancellationToken token)
        {
            var sessionContext = manager.GetSupportedSerializerContext(response);
            using var request = new HttpRequestMessage(HttpMethod.Put, manager.Options.Path.Relative)
            {
                Content = new SerializedSessionHttpContent(manager.Serializer, State, sessionContext)
            };

            manager.AddSessionCookieToHeader(request, State.SessionID);

            using var result = await manager.BackchannelClient.SendAsync(request, token);

            manager.LogCommitResponse(result.StatusCode);

            result.EnsureSuccessStatusCode();
        }
    }
}
