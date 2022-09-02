// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class RemoteAppClientExtensions
{
    /// <summary>
    /// Add configuration for connecting to a remote app for System.Web extensions that require a remote
    /// ASP.NET app such as remote app authentication or remote app session sharing.
    /// </summary>
    public static ISystemWebAdapterBuilder AddRemoteAppClient(this ISystemWebAdapterBuilder builder, Action<ISystemWebAdapterRemoteClientAppBuilder> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddOptions<RemoteAppClientOptions>()
            .ValidateDataAnnotations();

        builder.Services.AddTransient<HandleVirtualDirectoryHandler>();
        builder.Services.AddHttpClient(RemoteConstants.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value;

                if (options.BackchannelHandler is { } handler)
                {
                    return handler;
                }

                // Disable cookies in the HTTP client because the service will manage the cookie header directly
                return new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };
            })
            .AddHttpMessageHandler<HandleVirtualDirectoryHandler>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RemoteAppClientOptions>>().Value;

                client.BaseAddress = options.RemoteAppUrl;
                client.DefaultRequestHeaders.Add(options.ApiKeyHeader, options.ApiKey);
            });

        configure(new Builder(builder.Services));

        return builder;
    }

    public static ISystemWebAdapterRemoteClientAppBuilder Configure(this ISystemWebAdapterRemoteClientAppBuilder builder, Action<RemoteAppClientOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddOptions<RemoteAppClientOptions>()
            .Configure(configure);

        return builder;
    }

    /// <summary>
    /// <see cref="HttpClient.BaseAddress"/> is set automatically, but if this is supposed to have a path, then that gets
    /// lost. This handler will append that back if necessary.
    /// </summary>
    private class HandleVirtualDirectoryHandler : DelegatingHandler
    {
        private readonly string? _path;

        public HandleVirtualDirectoryHandler(IOptions<RemoteAppClientOptions> options)
        {
            _path = options.Value.RemoteAppUrl.AbsolutePath;

            if (_path == "/")
            {
                _path = null;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_path is not null && request.RequestUri is { } uri)
            {
                var builder = new UriBuilder(uri);
                builder.Path = $"{_path}{builder.Path}";
                request.RequestUri = builder.Uri;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private class Builder : ISystemWebAdapterRemoteClientAppBuilder
    {
        public Builder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
