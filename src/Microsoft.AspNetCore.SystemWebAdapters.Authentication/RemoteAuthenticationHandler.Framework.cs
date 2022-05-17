// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

internal sealed class RemoteAuthenticationHandler : HttpTaskAsyncHandler
{
    private readonly RemoteAuthenticationOptions _options;

    public override bool IsReusable => true;

    public RemoteAuthenticationHandler(RemoteAuthenticationOptions options)
    {
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            throw new ArgumentOutOfRangeException("API key must not be empty.");
        }

        _options = options;
    }

    public override async Task ProcessRequestAsync(HttpContext context)
    {
        if (!string.Equals(_options.ApiKey, context.Request.Headers.Get(_options.ApiKeyHeader), StringComparison.Ordinal))
        {
            context.Response.StatusCode = 401;
        }
        else
        {
            /*
            var readOnly = bool.TryParse(context.Request.Headers.Get(RemoteAppSessionStateOptions.ReadOnlyHeaderName), out var result) && result;

            // Dispatch the work depending on the HTTP method used
            var method = context.Request.HttpMethod;
            if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                await StoreSessionStateAsync(context).ConfigureAwait(false);
            }
            else if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await GetSessionStateAsync(context, !readOnly).ConfigureAwait(false);
            }
            else
            {
                // HTTP methods other than GET (read) or PUT (write) are not accepted
                context.Response.StatusCode = 405; // Method not allowed
            }
            */
        }

        context.ApplicationInstance.CompleteRequest();
    }
}
