// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace System.Web.Hosting;

internal sealed class HostingEnvironmentAccessor
{
    private static HostingEnvironmentAccessor? _current;

    private readonly IOptions<SystemWebAdaptersOptions> _options;

    public HostingEnvironmentAccessor(IServiceProvider services, IOptions<SystemWebAdaptersOptions> options)
    {
        Services = services;
        _options = options;
    }

    public IServiceProvider Services { get; }

    [AllowNull]
    public static HostingEnvironmentAccessor Current
    {
        get => _current ?? throw new InvalidOperationException("Hosting environment is only available when a host is running.");
        set
        {
            if (_current is not null && value is not null)
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

    internal SystemWebAdaptersOptions Options => _options.Value;
}
