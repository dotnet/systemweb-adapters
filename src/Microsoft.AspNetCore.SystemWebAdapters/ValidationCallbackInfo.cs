// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

internal sealed class ValidationCallbackInfo
{
    internal readonly HttpCacheValidateHandler handler;
    internal readonly Object data;

    internal ValidationCallbackInfo(HttpCacheValidateHandler handler, Object data)
    {
        this.handler = handler;
        this.data = data;
    }
}
