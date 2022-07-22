// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal static class AuthenticationConstants
{
    public const string ForwardedHostHeaderName = "x-forwarded-host";
    public const string ForwardedProtoHeaderName = "x-forwarded-proto";
    public const string MigrationAuthenticateRequestHeaderName = "x-migration-authenticate";
    public const string OriginalUrlQueryParamName = "original-url";
    public const string DefaultEndpoint = "/systemweb-adapters/authenticate";
}
