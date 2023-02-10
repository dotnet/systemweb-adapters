// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web.Caching;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace Microsoft.Extensions.DependencyInjection;

internal static class HttpRuntimeFactory
{
    public static IHttpRuntime Create(IServiceProvider serviceProvider)
    {
        if (NativeMethods.IsAspNetCoreModuleLoaded())
        {
            var config = NativeMethods.HttpGetApplicationProperties();
            return new IISHttpRuntime(config, serviceProvider);
        }

        return new DefaultHttpRuntime(serviceProvider);
    }

    internal abstract class BaseHttpRuntime 
    {
        protected BaseHttpRuntime(IServiceProvider serviceProvider)
        {
            Cache = serviceProvider.GetRequiredService<Cache>();
        }

        public Cache Cache { get; }
    }

    internal class DefaultHttpRuntime : BaseHttpRuntime, IHttpRuntime
    {
        public DefaultHttpRuntime(IServiceProvider sp) : base(sp)
        {
        }

        public string AppDomainAppVirtualPath => "/";

        public string AppDomainAppPath => AppContext.BaseDirectory;
    }

    internal class IISHttpRuntime : BaseHttpRuntime, IHttpRuntime
    {
        private readonly NativeMethods.IISConfigurationData _config;

        public IISHttpRuntime(NativeMethods.IISConfigurationData config, IServiceProvider sp) : base(sp)
        {
            _config = config;
        }

        public string AppDomainAppVirtualPath => _config.pwzVirtualApplicationPath;

        public string AppDomainAppPath => _config.pwzFullApplicationPath;
    }
}
