// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class ReadOnlySessionHandler : VersionedSessionHandler, IReadOnlySessionState
{
    private readonly ISessionSerializer _serializer;

    public override bool IsReusable => true;

    public ReadOnlySessionHandler(ISessionSerializer serializer)
    {
        _serializer = serializer;
    }

    public override async Task ProcessRequestAsync(HttpContextBase context, SessionSerializerContext sessionContext, CancellationToken token)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = 200;

        using var wrapper = new HttpSessionStateBaseWrapper(context.Session);

        await _serializer.SerializeAsync(wrapper, sessionContext, context.Response.OutputStream, context.Response.ClientDisconnectedToken);
    }
}
