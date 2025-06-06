// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed partial class ReadWriteSessionHandler : VersionedSessionHandler, IRequiresSessionState, IRequireBufferlessStream
{
    private readonly ISessionSerializer _serializer;
    private readonly ILogger _logger;

    public ReadWriteSessionHandler(ISessionSerializer serializer, ILogger logger)
    {
        _serializer = serializer;
        _logger = logger;
    }

    public override async Task ProcessRequestAsync(HttpContextBase context, SessionSerializerContext sessionContext, CancellationToken token)
    {
        await SendSessionAsync(context, sessionContext, token).ConfigureAwait(false);

        if (await RetrieveUpdatedSessionAsync(context, token))
        {
            await SendSessionWriteResultAsync(context.Response, Results.Succeeded, token);
        }
        else
        {
            await SendSessionWriteResultAsync(context.Response, Results.NoSessionData, token);
        }
    }

    private async Task SendSessionAsync(HttpContextBase context, SessionSerializerContext sessionContext, CancellationToken token)
    {
        // Send the initial snapshot of session data
        context.Response.ContentType = "text/event-stream";
        context.Response.StatusCode = 200;

        using var wrapper = new HttpSessionStateBaseWrapper(context.Session);

        await _serializer.SerializeAsync(wrapper, sessionContext, context.Response.OutputStream, token);

        // Ensure to call HttpResponse.FlushAsync to flush the request itself, and not context.Response.OutputStream.FlushAsync()
        await context.Response.OutputStream.FlushAsync(token);
        await context.Response.FlushAsync();
    }

    private async Task<bool> RetrieveUpdatedSessionAsync(HttpContextBase context, CancellationToken token)
    {
        // This will wait for data to be pushed for the session info to be committed
        using var stream = context.Request.GetBufferlessInputStream();

        using var deserialized = await _serializer.DeserializeAsync(stream, token);

        if (deserialized is { })
        {
            deserialized.CopyTo(_logger, context.Session);
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
