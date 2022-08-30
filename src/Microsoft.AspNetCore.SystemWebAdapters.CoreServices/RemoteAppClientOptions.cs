// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Options for connecting to a remote app.
/// </summary>
public class RemoteAppClientOptions
{
    // Matches an empty GUID (32 0s) with dashes, parentheses, and braces optional
    private const string EmptyGuidRegex =
        "[({]?" + // Optional starting brace or parenthesis
        "0{8}-?" + // 8 0s followed, optionally, by a -
        "0{4}-?" + // 4 0s followed, optionally, by a -
        "0{4}-?" + // 4 0s followed, optionally, by a -
        "0{4}-?" + // 4 0s followed, optionally, by a -
        "0{12}-?" + // 12 0s followed, optionally, by a -
        "[})]?";      // Optional closing brace or parenthesis

    // Matches a GUID with dashes, parentheses, and braces optional
    private const string GuidRegex =
        "[({]?" + // Optional starting brace or parenthesis
        "[0-9a-fA-F]{8}-?" + // 8 hex digits followed, optionally, by a -
        "[0-9a-fA-F]{4}-?" + // 4 hex digits followed, optionally, by a -
        "[0-9a-fA-F]{4}-?" + // 4 hex digits followed, optionally, by a -
        "[0-9a-fA-F]{4}-?" + // 4 hex digits followed, optionally, by a -
        "[0-9a-fA-F]{12}-?" + // 12 hex digits followed, optionally, by a -
        "[})]?";              // Optional closing brace or parenthesis

    // Matches a GUID that is not an empty GUID
    private const string NonEmptyGuidRegex =
        $"^" + // Beginning of string anchor
        $"(?!{EmptyGuidRegex})" + // Looking ahead does *not* match empty GUID
        $"{GuidRegex}$";          // Matches GUID

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    [Required]
    public string ApiKeyHeader { get; set; } = RemoteConstants.ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
    [Required, RegularExpression(NonEmptyGuidRegex, ErrorMessage = "API Key must be 32 hex characters (for example a GUID)")]
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteAppUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets an <see cref="HttpMessageHandler"/> to use for making requests to the remote app.
    /// </summary>
    public HttpMessageHandler? BackchannelHandler { get; set; }
}
