// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class ReadOnlySessionHandler : HttpTaskAsyncHandler, IReadOnlySessionState
{
    private readonly ISessionSerializer _serializer;

    public override bool IsReusable => true;

    public ReadOnlySessionHandler(ISessionSerializer serializer)
    {
        _serializer = serializer;
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        await ProcessRequestAsync(new HttpContextWrapper(context));
        context.ApplicationInstance.CompleteRequest();
    }

    public async Task ProcessRequestAsync(HttpContextBase context)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = 200;

        await _serializer.SerializeAsync(context.Session, context.Response.OutputStream, context.Response.ClientDisconnectedToken);
    }
}
