// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class GetWriteableSessionHandler : HttpTaskAsyncHandler, IRequiresSessionState
{
    private const byte EndOfFrame = (byte)'\n';

    private readonly ISessionSerializer _serializer;
    private readonly ILockedSessionCache _cache;

    public GetWriteableSessionHandler(ISessionSerializer serializer, ILockedSessionCache cache)
    {
        _serializer = serializer;
        _cache = cache;
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(context.Session.Timeout));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.Response.ClientDisconnectedToken);

        await ProcessRequestAsync(new HttpContextWrapper(context), cts.Token).ConfigureAwait(false);

        context.ApplicationInstance.CompleteRequest();
    }

    public async Task ProcessRequestAsync(HttpContextBase context, CancellationToken token)
    {
        // If session data is retrieved exclusively, then it needs sent to the client and
        // this request needs to remain open while waiting for the client to either send updates
        // or release the session without updates.

        // Add the session to the cache. Disposing this will remove it from the cache.
        using var cts = new CancellationTokenSource();
        using var _ = _cache.Register(context.Session, () => cts.Cancel());

        // Send the initial snapshot of session data
        context.Response.ContentType = "text/event-stream";
        context.Response.StatusCode = 200;

        await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, token);

        // Delimit the json body with a new line to mark the end of content
        context.Response.OutputStream.WriteByte(EndOfFrame);

        // Ensure to call HttpResponse.FlushAsync to flush the request itself, and not context.Response.OutputStream.FlushAsync()
        await context.Response.FlushAsync();

        // Wait for up to request timeout for updated session state to be written.
        // We send down heartbeats to ensure the request disconnected token fires correctly
        using var waitToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
        var heartbeatDelay = TimeSpan.FromMilliseconds(20);

        while (!waitToken.IsCancellationRequested)
        {
            await Task.Delay(heartbeatDelay, waitToken.Token);
            context.Response.OutputStream.WriteByte(EndOfFrame);

            // Ensure to call HttpResponse.FlushAsync to flush the request itself, and not context.Response.OutputStream.FlushAsync()
            await context.Response.FlushAsync();
        }
    }
}

