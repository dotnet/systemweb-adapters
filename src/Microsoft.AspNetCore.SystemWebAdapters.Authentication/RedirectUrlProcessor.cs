// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

/// <summary>
/// Processes remote authentication results to fix-up 'ReturnUrl' parameters
/// given in redirect responses so that they will return the user to the correct
/// original URL instead of to the remote authentication endpoint.
/// </summary>
internal class RedirectUrlProcessor : IRemoteAuthenticationResultProcessor
{
    private const string LocationHeaderName = "Location";
    private const string ReturnUrlQueryStringName = "ReturnUrl";

    /// <summary>
    /// Updates response headers in a remote authentication result so that any
    /// 'Location' headers will include a ReturnUrl query string corresponding to the
    /// URL of the original request rather than the URL of the remote authentication endpoint.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task ProcessAsync(RemoteAuthenticationResult result, HttpContext context)
    {
        if (result.ResponseHeaders.TryGetValue(LocationHeaderName, out var locationHeaders))
        {
            // Look for any Location headers with URLs including a ReturnUrl query string as their value
            var headerValues = locationHeaders.ToArray();
            for (var i = 0; i < headerValues.Length; i++)
            {
                if (Uri.TryCreate(headerValues[i], UriKind.RelativeOrAbsolute, out var redirectLocation))
                {
                    var queryStrings = QueryHelpers.ParseQuery(redirectLocation.Query);
                    if (queryStrings.ContainsKey(ReturnUrlQueryStringName))
                    {
                        // Get the plain redirect URL without the query string
                        var redirectWithoutQuery = redirectLocation.ToString().Replace(redirectLocation.Query, string.Empty);

                        // Update the query strings to use the original request path as the ReturnUrl query string
                        queryStrings[ReturnUrlQueryStringName] = context.Request.Path.Value;

                        // .NET Core 3.1 doesn't have a QueryHelpers.AddQueryString that takes an argument of type Dictionary<string, StringValues>
                        // so we have to convert the type.
                        var queryStringDictionary = queryStrings.ToDictionary<KeyValuePair<string, StringValues>, string, string?>(kvp => kvp.Key, kvp => kvp.Value.ToString());

                        var updatedRedirect = QueryHelpers.AddQueryString(redirectWithoutQuery, queryStringDictionary);
                        headerValues[i] = updatedRedirect.ToString();
                    }
                }
            }
            result.ResponseHeaders[LocationHeaderName] = headerValues;
        }

        return Task.CompletedTask;
    }
}
