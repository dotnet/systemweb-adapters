// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class RemoteSessionModule : RemoteModule
{
    public RemoteSessionModule(IOptions<RemoteAppSessionStateServerOptions> sessionOptions, IOptions<RemoteAppServerOptions> remoteAppOptions, ILockedSessionCache cache, ISessionSerializer serializer)
        : base(remoteAppOptions)
    {
        if (sessionOptions is null)
        {
            throw new ArgumentNullException(nameof(sessionOptions));
        }

        var options = sessionOptions.Value;

        var readonlyHandler = new ReadOnlySessionHandler(serializer);
        var writeableHandler = new GetWriteableSessionHandler(serializer, cache);
        var saveHandler = new StoreSessionStateHandler(cache, options.CookieName);

        Register(HttpMethod.Get, context => GetIsReadonly(context.Request) ? readonlyHandler : writeableHandler);
        Register(HttpMethod.Put, context => saveHandler);

        Path = options.SessionEndpointPath;

        static bool GetIsReadonly(HttpRequestBase request)
            => bool.TryParse(request.Headers.Get(SessionConstants.ReadOnlyHeaderName), out var result) && result;
    }

    protected override string Path { get; }
}
