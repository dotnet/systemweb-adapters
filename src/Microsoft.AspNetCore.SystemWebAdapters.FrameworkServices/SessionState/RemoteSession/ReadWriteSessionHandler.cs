// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed partial class ReadWriteSessionHandler : HttpTaskAsyncHandler, IRequiresSessionState, IRequireBufferlessStream
{
    private readonly ISessionSerializer _serializer;

    public ReadWriteSessionHandler(ISessionSerializer serializer)
    {
        _serializer = serializer;
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(context.Session.Timeout));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.Response.ClientDisconnectedToken);

        await SendSessionAsync(new HttpContextWrapper(context), cts.Token).ConfigureAwait(false);

        context.ApplicationInstance.CompleteRequest();
    }

    public async Task SendSessionAsync(HttpContextBase context, CancellationToken token)
    {
        // Send the initial snapshot of session data
        context.Response.ContentType = "text/event-stream";
        context.Response.StatusCode = 200;

        using var wrapper = new HttpSessionStateBaseWrapper(context.Session);

        await _serializer.SerializeAsync(wrapper, context.Response.OutputStream, token);

        // Ensure to call HttpResponse.FlushAsync to flush the request itself, and not context.Response.OutputStream.FlushAsync()
        await context.Response.FlushAsync();

        // This will wait for data to be pushed for the session info to be committed
        using var stream = context.Request.GetBufferlessInputStream();

        using var deserialized = await _serializer.DeserializeAsync(stream, token);

        if (deserialized is { })
        {
            deserialized.CopyTo(context.Session);

            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new SessionPostResult() { Success = true }, ResultContext.Default.SessionPostResult, token);
        }
        else
        {
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new SessionPostResult() { Success = false, Message = "No session data was supplied for commit" }, ResultContext.Default.SessionPostResult, token);
        }
    }

    private class SessionPostResult
    {
        public bool Success { get; set; }

        public string? Message { get; set; }
    }

    [JsonSerializable(typeof(SessionPostResult))]
    private partial class ResultContext : JsonSerializerContext
    {
    }
}

