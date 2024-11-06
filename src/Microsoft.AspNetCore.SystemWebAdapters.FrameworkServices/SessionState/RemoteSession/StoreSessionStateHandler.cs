// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class StoreSessionStateHandler : HttpTaskAsyncHandler
{
    private readonly ILockedSessionCache _cache;
    private readonly string _cookieName;

    internal static class Messages
    {
        public const string NoSessionId = "No session ID found";
        public const string SessionNotFound = "Could not find session";
        public const string DeserializationFailed = "Failed to deserialize session state";
        public const string SessionAlreadyUpdated = "Session has already been updated";
    }

    public StoreSessionStateHandler(ILockedSessionCache cache, string cookieName)
    {
        _cache = cache;
        _cookieName = cookieName;
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        await ProcessRequestAsync(new HttpContextWrapper(context));
        context.ApplicationInstance.CompleteRequest();
    }

    public async Task ProcessRequestAsync(HttpContextBase context)
    {
        var sessionId = context.Request.Cookies[_cookieName]?.Value;

        // Check that the request has a session ID
        if (sessionId is null)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = Messages.NoSessionId;
        }
        else
        {
            using var content = context.Request.GetInputStream();
            var result = await _cache.SaveAsync(sessionId, content, context.Response.ClientDisconnectedToken);

            if (result is SessionSaveResult.Success)
            {
                context.Response.StatusCode = 200;
            }
            else
            {
                var description = result switch
                {
                    SessionSaveResult.SessionNotFound => Messages.SessionNotFound,
                    SessionSaveResult.DeserializationError => Messages.DeserializationFailed,
                    SessionSaveResult.AlreadyUpdated => Messages.SessionAlreadyUpdated,
                    _ => $"Unknown result: {result}",
                };

                context.Response.StatusDescription = description;
                context.Response.StatusCode = 400;
            }
        }
    }
}


