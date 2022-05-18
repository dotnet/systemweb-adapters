// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal class RedirectUrlProcessor : IRemoteAuthenticateResultProcessor
{
    private const string LocationHeaderName = "Location";

    public Task ProcessAsync(RemoteAuthenticationResult result, HttpContext context)
    {
        if (result.ResponseHeaders.TryGetValue(LocationHeaderName, out var locationHeaders))
        {
            var headerValues = locationHeaders.ToArray();
            for (var i = 0; i < headerValues.Length; i++)
            {
                if (Uri.TryCreate(headerValues[i], UriKind.RelativeOrAbsolute, out var redirectLocation))
                {
                    var queryStrings = QueryHelpers.ParseQuery(redirectLocation.Query);
                    if (queryStrings.ContainsKey("ReturnUrl"))
                    {
                        queryStrings["ReturnUrl"] = context.Request.Path.Value;
                        var redirectWithoutQuery = redirectLocation.ToString().Replace(redirectLocation.Query, string.Empty);

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
