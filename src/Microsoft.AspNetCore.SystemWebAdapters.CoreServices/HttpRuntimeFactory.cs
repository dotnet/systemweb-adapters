// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

internal static class HttpRuntimeFactory
{
    public static IHttpRuntime Create()
    {
        if (NativeMethods.IsAspNetCoreModuleLoaded())
        {
            var config = NativeMethods.HttpGetApplicationProperties();

            return new IISHttpRuntime(config);
        }

        return new DefaultHttpRuntime();
    }

    internal class DefaultHttpRuntime : IHttpRuntime
    {
        public string AppDomainAppVirtualPath => "/";

        public string AppDomainAppPath => AppContext.BaseDirectory;
    }

    internal class IISHttpRuntime : IHttpRuntime
    {
        private readonly NativeMethods.IISConfigurationData _config;

        public IISHttpRuntime(NativeMethods.IISConfigurationData config)
        {
            _config = config;
        }

        public string AppDomainAppVirtualPath => _config.pwzVirtualApplicationPath;

        public string AppDomainAppPath => _config.pwzFullApplicationPath;
    }
}
