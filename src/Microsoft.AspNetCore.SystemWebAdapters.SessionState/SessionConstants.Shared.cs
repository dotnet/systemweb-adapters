// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

internal static class SessionConstants
{
    public const string ReadOnlyHeaderName = "X-SystemWebAdapter-RemoteAppSession-ReadOnly";

    public const string SessionEndpointPath = "/systemweb-adapters/session";

    public const string DefaultCookieName = "ASP.NET_SessionId";
}
