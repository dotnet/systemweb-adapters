// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// A feature that exposes ability to raise events on the current <see cref="HttpApplication"/>. 
/// See https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication#remarks for details on how this worked in .NET Framework.
/// </summary>
internal interface IHttpApplicationEventsFeature
{
    ValueTask RaiseBeginRequestAsync(CancellationToken token);

    ValueTask RaiseAuthenticateRequestAsync(CancellationToken token);

    ValueTask RaisePostAuthenticateRequestAsync(CancellationToken token);

    ValueTask RaiseAuthorizeRequestAsync(CancellationToken token);

    ValueTask RaisePostAuthorizeRequestAsync(CancellationToken token);

    ValueTask RaiseResolveRequestCacheAsync(CancellationToken token);

    ValueTask RaisePostResolveRequestCacheAsync(CancellationToken token);

    ValueTask RaiseMapRequestHandlerAsync(CancellationToken token);

    ValueTask RaisePostMapRequestHandlerAsync(CancellationToken token);

    ValueTask RaiseAcquireRequestStateAsync(CancellationToken token);

    ValueTask RaisePostAcquireRequestStateAsync(CancellationToken token);

    ValueTask RaisePreRequestHandlerExecuteAsync(CancellationToken token);

    ValueTask RaisePostRequestHandlerExecuteAsync(CancellationToken token);

    ValueTask RaiseReleaseRequestStateAsync(CancellationToken token);

    ValueTask RaisePostReleaseRequestStateAsync(CancellationToken token);

    ValueTask RaiseUpdateRequestCacheAsync(CancellationToken token);

    ValueTask RaisePostUpdateRequestCacheAsync(CancellationToken token);

    ValueTask RaiseLogRequestAsync(CancellationToken token);

    ValueTask RaisePostLogRequestAsync(CancellationToken token);

    ValueTask RaiseEndRequestAsync(CancellationToken token);

    ValueTask RaisePreSendRequestHeaders(CancellationToken token);

    ValueTask RaiseErrorAsync(CancellationToken token);

    ValueTask RaiseRequestCompletedAsync(CancellationToken token);

    ValueTask RaiseSessionStart(CancellationToken token);

    ValueTask RaiseSessionEnd(CancellationToken token);
}

#endif
