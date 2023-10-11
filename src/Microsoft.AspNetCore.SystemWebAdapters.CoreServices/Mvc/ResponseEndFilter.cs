// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.Mvc;

internal partial class ResponseEndFilter : IActionFilter
{
    private readonly ILogger<ResponseEndFilter> _logger;

    [LoggerMessage(0, LogLevel.Trace, "Clearing MVC result since HttpResponse.End() was called")]
    private partial void LogClearingResult();

    public ResponseEndFilter(ILogger<ResponseEndFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not null && context.HttpContext.Features.Get<IHttpResponseEndFeature>() is { IsEnded: true })
        {
            LogClearingResult();
            context.Result = null;
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
