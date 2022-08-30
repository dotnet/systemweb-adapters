// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

internal class ApiKeyAttribute: RegularExpressionAttribute
{
    private const string DefaultErrorMessage = "API Key must be 32 hex characters (for example a GUID)";

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

    public ApiKeyAttribute() : base(NonEmptyGuidRegex)
    {
        ErrorMessage = DefaultErrorMessage;
    }
}
