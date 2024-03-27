// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Web.Hosting;

internal sealed class HostingEnvironmentAccessor : IDisposable
{
    private static HttpContextAccessor? _defaultHttpContextAccessor;
    private static HostingEnvironmentAccessor? _current;

    private readonly IOptions<SystemWebAdaptersOptions> _options;
    private readonly IHttpContextAccessor? _accessor;

    public HostingEnvironmentAccessor(IServiceProvider services, IOptions<SystemWebAdaptersOptions> options)
    {
        Services = services;
        _accessor = services.GetService<IHttpContextAccessor>();
        _options = options;

        Current = this;
    }

    public IServiceProvider Services { get; }

    [AllowNull]
    public static HostingEnvironmentAccessor Current
    {
        get => _current ?? throw new InvalidOperationException("Hosting environment is only available when a host is running.");
        private set
        {
            if (_current is not null && value is not null && !ReferenceEquals(_current, value))
            {
                throw new InvalidOperationException("Hosting environment is already set");
            }

            _current = value;
        }
    }

    public static bool TryGet([MaybeNullWhen(false)] out HostingEnvironmentAccessor current)
    {
        current = _current;
        return current is not null;
    }

    public void Dispose()
    {
        Current = null;
    }

    /// <summary>
    /// Gets an <see cref="IHttpContextAccessor"/> that is either registered to the current hosting runtime or the default one via <see cref="HttpContextAccessor"/>.  
    /// </summary>
    public static IHttpContextAccessor HttpContextAccessor
    {
        get
        {
            if (_current?._accessor is { } current)
            {
                return current;
            }

            if (_defaultHttpContextAccessor is null)
            {
                Interlocked.CompareExchange(ref _defaultHttpContextAccessor, new(), null);
            }

            return _defaultHttpContextAccessor;
        }
    }

    internal SystemWebAdaptersOptions Options => _options.Value;
}
