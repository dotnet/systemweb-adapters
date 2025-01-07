// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class SessionPostResult
{
    [JsonPropertyName("s")]
    public bool Success { get; set; }

    [JsonPropertyName("m")]
    public string? Message { get; set; }
}
