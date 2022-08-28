// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAppAuthenticationServerOptions
{
    [Required]
    public string AuthenticationEndpointPath { get; set; } = AuthenticationConstants.DefaultEndpoint;
}
