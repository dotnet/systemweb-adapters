// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal sealed class RemoteSessionModule : RemoteModule
{
    public RemoteSessionModule(IOptions<RemoteAppSessionStateServerOptions> sessionOptions, IOptions<RemoteAppServerOptions> remoteAppOptions, ILoggerFactory loggerFactory, ILockedSessionCache cache, ISessionSerializer serializer)
        : base(remoteAppOptions)
    {
        if (sessionOptions is null)
        {
            throw new ArgumentNullException(nameof(sessionOptions));
        }

        var options = sessionOptions.Value;

        Path = options.SessionEndpointPath;

        var readonlyHandler = new ReadOnlySessionHandler(serializer);
        var writeableHandler = new GetWriteableSessionHandler(serializer, cache);
        var persistHandler = new ReadWriteSessionHandler(serializer, loggerFactory.CreateLogger<ReadWriteSessionHandler>());
        var saveHandler = new StoreSessionStateHandler(cache, options.CookieName);

        MapGet(context => GetIsReadonly(context.Request) ? readonlyHandler : writeableHandler);

        MapPut(context => saveHandler);
        MapPost(context => persistHandler);

        static bool GetIsReadonly(HttpRequestBase request)
            => bool.TryParse(request.Headers.Get(SessionConstants.ReadOnlyHeaderName), out var result) && result;
    }

    protected override string Path { get; }
}
