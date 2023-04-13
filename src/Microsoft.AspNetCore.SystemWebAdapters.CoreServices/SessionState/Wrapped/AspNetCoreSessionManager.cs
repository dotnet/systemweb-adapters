// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped;

internal class AspNetCoreSessionManager : ISessionManager
{
    private readonly ISessionKeySerializer _serializer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<SessionSerializerOptions> _options;

    public AspNetCoreSessionManager(ICompositeSessionKeySerializer serializer, ILoggerFactory loggerFactory, IOptions<SessionSerializerOptions> options)
    {
        _serializer = serializer;
        _loggerFactory = loggerFactory;
        _options = options;
    }

    public Task<ISessionState> CreateAsync(HttpContextCore context, SessionAttribute metadata)
        => Task.FromResult<ISessionState>(new AspNetCoreSessionState(context.Session, _serializer, _loggerFactory, metadata.IsReadOnly, _options.Value.ThrowOnUnknownSessionKey));
}
