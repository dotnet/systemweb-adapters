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

        var contextWrapper = new HttpContextWrapper(context);

        await SendSessionAsync(contextWrapper, cts.Token).ConfigureAwait(false);

        if (await RetrieveUpdatedSessionAsync(contextWrapper, cts.Token))
        {
            await SendSessionWriteResultAsync(contextWrapper.Response, Results.Succeeded, cts.Token);
        }
        else
        {
            await SendSessionWriteResultAsync(contextWrapper.Response, Results.NoSessionData, cts.Token);
        }

        context.ApplicationInstance.CompleteRequest();
    }

    private async Task SendSessionAsync(HttpContextBase context, CancellationToken token)
    {
        // Send the initial snapshot of session data
        context.Response.ContentType = "text/event-stream";
        context.Response.StatusCode = 200;

        using var wrapper = new HttpSessionStateBaseWrapper(context.Session);

        await _serializer.SerializeAsync(wrapper, context.Response.OutputStream, token);

        // Ensure to call HttpResponse.FlushAsync to flush the request itself, and not context.Response.OutputStream.FlushAsync()
        await context.Response.FlushAsync();
    }

    private async Task<bool> RetrieveUpdatedSessionAsync(HttpContextBase context, CancellationToken token)
    {
        // This will wait for data to be pushed for the session info to be committed
        using var stream = context.Request.GetBufferlessInputStream();

        using var deserialized = await _serializer.DeserializeAsync(stream, token);

        if (deserialized is { })
        {
            deserialized.CopyTo(context.Session);
            return true;
        }
        else
        {
            return false;
        }
    }

    private static Task SendSessionWriteResultAsync(HttpResponseBase response, SessionPostResult result, CancellationToken token)
        => JsonSerializer.SerializeAsync(response.OutputStream, result, SessionPostResultContext.Default.SessionPostResult, token);

    [JsonSerializable(typeof(SessionPostResult))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
    private partial class SessionPostResultContext : JsonSerializerContext
    {
    }

    private static class Results
    {
        public static SessionPostResult Succeeded { get; } = new() { Success = true };

        public static SessionPostResult NoSessionData { get; } = new() { Success = false, Message = "No session data was supplied for commit" };
    }
}
