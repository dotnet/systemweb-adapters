// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class CompositeHttpApplicationEventsFeature: IHttpApplicationEventsFeature
{
    private readonly IHttpApplicationEventsFeature _feature1;
    private readonly IHttpApplicationEventsFeature _feature2;

    public CompositeHttpApplicationEventsFeature(IHttpApplicationEventsFeature feature1, IHttpApplicationEventsFeature feature2)
    {
        _feature1 = feature1;
        _feature2 = feature2;
    }

    public async ValueTask RaiseAcquireRequestStateAsync(CancellationToken token)
    {
        await _feature1.RaiseAcquireRequestStateAsync(token);
        await _feature2.RaiseAcquireRequestStateAsync(token);
    }

    public async ValueTask RaiseAuthenticateRequestAsync(CancellationToken token)
    {
        await _feature1.RaiseAuthenticateRequestAsync(token);
        await _feature2.RaiseAuthenticateRequestAsync(token);
    }

    public async ValueTask RaiseAuthorizeRequestAsync(CancellationToken token)
    {
        await _feature1.RaiseAuthorizeRequestAsync(token);
        await _feature2.RaiseAuthorizeRequestAsync(token);
    }

    public async ValueTask RaiseBeginRequestAsync(CancellationToken token)
    {
        await _feature1.RaiseBeginRequestAsync(token);
        await _feature2.RaiseBeginRequestAsync(token);
    }

    public async ValueTask RaiseEndRequestAsync(CancellationToken token)
    {
        await _feature1.RaiseEndRequestAsync(token);
        await _feature2.RaiseEndRequestAsync(token);
    }

    public async ValueTask RaiseErrorAsync(CancellationToken token)
    {
        await _feature1.RaiseErrorAsync(token);
        await _feature2.RaiseErrorAsync(token);
    }

    public async ValueTask RaiseLogRequestAsync(CancellationToken token)
    {
        await _feature1.RaiseLogRequestAsync(token);
        await _feature2.RaiseLogRequestAsync(token);
    }

    public async ValueTask RaiseMapRequestHandlerAsync(CancellationToken token)
    {
        await _feature1.RaiseMapRequestHandlerAsync(token);
        await _feature2.RaiseMapRequestHandlerAsync(token);
    }

    public async ValueTask RaisePostAcquireRequestStateAsync(CancellationToken token)
    {
        await _feature1.RaisePostAcquireRequestStateAsync(token);
        await _feature2.RaisePostAcquireRequestStateAsync(token);
    }

    public async ValueTask RaisePostAuthenticateRequestAsync(CancellationToken token)
    {
        await _feature1.RaisePostAuthenticateRequestAsync(token);
        await _feature2.RaisePostAuthenticateRequestAsync(token);
    }

    public async ValueTask RaisePostAuthorizeRequestAsync(CancellationToken token)
    {
        await _feature1.RaisePostAuthorizeRequestAsync(token);
        await _feature2.RaisePostAuthorizeRequestAsync(token);
    }

    public async ValueTask RaisePostLogRequestAsync(CancellationToken token)
    {
        await _feature1.RaisePostLogRequestAsync(token);
        await _feature2.RaisePostLogRequestAsync(token);
    }

    public async ValueTask RaisePostMapRequestHandlerAsync(CancellationToken token)
    {
        await _feature1.RaisePostMapRequestHandlerAsync(token);
        await _feature2.RaisePostMapRequestHandlerAsync(token);
    }

    public async ValueTask RaisePostReleaseRequestStateAsync(CancellationToken token)
    {
        await _feature1.RaisePostReleaseRequestStateAsync(token);
        await _feature2.RaisePostReleaseRequestStateAsync(token);
    }

    public async ValueTask RaisePostRequestHandlerExecuteAsync(CancellationToken token)
    {
        await _feature1.RaisePostRequestHandlerExecuteAsync(token);
        await _feature2.RaisePostRequestHandlerExecuteAsync(token);
    }

    public async ValueTask RaisePostResolveRequestCacheAsync(CancellationToken token)
    {
        await _feature1.RaisePostResolveRequestCacheAsync(token);
        await _feature2.RaisePostResolveRequestCacheAsync(token);
    }

    public async ValueTask RaisePostUpdateRequestCacheAsync(CancellationToken token)
    {
        await _feature1.RaisePostUpdateRequestCacheAsync(token);
        await _feature2.RaisePostUpdateRequestCacheAsync(token);
    }

    public async ValueTask RaisePreRequestHandlerExecuteAsync(CancellationToken token)
    {
        await _feature1.RaisePreRequestHandlerExecuteAsync(token);
        await _feature2.RaisePreRequestHandlerExecuteAsync(token);
    }

    public async ValueTask RaisePreSendRequestHeaders(CancellationToken token)
    {
        await _feature1.RaisePreSendRequestHeaders(token);
        await _feature2.RaisePreSendRequestHeaders(token);
    }

    public async ValueTask RaiseReleaseRequestStateAsync(CancellationToken token)
    {
        await _feature1.RaiseReleaseRequestStateAsync(token);
        await _feature2.RaiseReleaseRequestStateAsync(token);
    }

    public async ValueTask RaiseRequestCompletedAsync(CancellationToken token)
    {
        await _feature1.RaiseRequestCompletedAsync(token);
        await _feature2.RaiseRequestCompletedAsync(token);
    }

    public async ValueTask RaiseResolveRequestCacheAsync(CancellationToken token)
    {
        await _feature1.RaiseResolveRequestCacheAsync(token);
        await _feature2.RaiseResolveRequestCacheAsync(token);
    }

    public async ValueTask RaiseSessionEnd(CancellationToken token)
    {
        await _feature1.RaiseSessionEnd(token);
        await _feature2.RaiseSessionEnd(token);
    }

    public async ValueTask RaiseSessionStart(CancellationToken token)
    {
        await _feature1.RaiseSessionStart(token);
        await _feature2.RaiseSessionStart(token);
    }

    public async ValueTask RaiseUpdateRequestCacheAsync(CancellationToken token)
    {
        await _feature1.RaiseUpdateRequestCacheAsync(token);
        await _feature2.RaiseUpdateRequestCacheAsync(token);
    }
}
