// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = Constants.ApiFromAspNet)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Need to delegate logger message from alternate logging system")]
internal sealed class LoggingTraceContext : ITraceContext
{
    private readonly ILogger _defaultCategory;
    private readonly ILoggerFactory _loggerFactory;

    public LoggingTraceContext(string defaultCategory, ILoggerFactory loggerFactory)
    {
        _defaultCategory = loggerFactory.CreateLogger(defaultCategory);
        _loggerFactory = loggerFactory;
    }

    public void Warn(string message, string? category, Exception? errorInfo)
        => GetLogger(category).LogWarning(errorInfo, message);

    public void Write(string message, string? category, Exception? errorInfo)
        => GetLogger(category).LogInformation(errorInfo, message);

    private ILogger GetLogger(string? category)
        => category is null ? _defaultCategory : _loggerFactory.CreateLogger(category);
}
