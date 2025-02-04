// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal abstract class VersionedSessionHandler : HttpTaskAsyncHandler
{
    public sealed override async Task ProcessRequestAsync(HttpContext context)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(context.Session.Timeout));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.Response.ClientDisconnectedToken);

        context.Response.Headers.Add(SessionConstants.SupportedVersion, SessionSerializerContext.Latest.SupportedVersion.ToString(CultureInfo.InvariantCulture));

        var sessionContext = SessionSerializerContext.Parse(context.Request.Headers.Get(SessionConstants.SupportedVersion));
        await ProcessRequestAsync(new HttpContextWrapper(context), sessionContext, cts.Token).ConfigureAwait(false);

        context.ApplicationInstance.CompleteRequest();
    }

    public abstract Task ProcessRequestAsync(HttpContextBase context, SessionSerializerContext sessionContext, CancellationToken token);
}

