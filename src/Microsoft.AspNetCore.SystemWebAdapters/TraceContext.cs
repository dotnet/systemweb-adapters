// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web;

public sealed class TraceContext
{
    private readonly ITraceContext? _context;

    internal TraceContext(HttpContext context)
    {
        _context = context.AsAspNetCore().RequestServices.GetService<ITraceContext>();
    }

    public bool IsEnabled { get; set; }

    public void Warn(string message) => Log(message, null, null, warn: true);

    public void Warn(string category, string message) => Log(message, category, null, warn: true);

    public void Warn(string category, string message, Exception errorInfo) => Log(message, category, errorInfo, warn: true);

    public void Write(string message) => Log(message, null, null, warn: false);

    public void Write(string category, string message) => Log(message, category, null, warn: false);

    public void Write(string category, string message, Exception errorInfo) => Log(message, category, errorInfo, warn: false);

    private void Log(string message, string? category, Exception? errorInfo, bool warn)
    {
        if (IsEnabled && _context is { })
        {
            if (warn)
            {
                _context.Warn(message, category, errorInfo);
            }
            else
            {
                _context.Write(message, category, errorInfo);
            }
        }
    }
}
