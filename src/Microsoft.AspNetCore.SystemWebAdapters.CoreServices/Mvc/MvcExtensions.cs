// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal static class MvcExtensions
{
    public static ISystemWebAdapterBuilder AddMvc(this ISystemWebAdapterBuilder builder)
    {
        builder.Services.TryAddTransient<ResponseEndFilter>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ResponseEndFilterOptions>());

        return builder;
    }

    private sealed class ResponseEndFilterOptions : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            // We want the check for HttpResponse.End() to be done as soon as possible after the action is run.
            // This will minimize any chance that output will be written which will fail since the response has completed.
            options.Filters.Add<ResponseEndFilter>(int.MaxValue);
        }
    }
}
