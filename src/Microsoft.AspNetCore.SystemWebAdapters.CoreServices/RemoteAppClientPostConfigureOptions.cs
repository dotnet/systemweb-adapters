// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal class RemoteAppClientPostConfigureOptions : IPostConfigureOptions<RemoteAppClientOptions>
{
    public void PostConfigure(string name, RemoteAppClientOptions options)
    {
        if (options.BackchannelClient is null)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            options.BackchannelClient = new HttpClient(
                options.BackchannelHandler
                // Disable cookies in the HTTP client because the service will manage the cookie header directly
                ?? new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Set base address and API key header based on options
            if (options.RemoteAppUrl is not null)
            {
                options.BackchannelClient.BaseAddress = options.RemoteAppUrl;
            }

            if (!string.IsNullOrEmpty(options.ApiKeyHeader))
            {
                options.BackchannelClient.DefaultRequestHeaders.Add(options.ApiKeyHeader, options.ApiKey);
            }
        }
    }
}
