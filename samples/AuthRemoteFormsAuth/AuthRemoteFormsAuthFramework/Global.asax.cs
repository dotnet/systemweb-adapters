using System;
using System.Configuration;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FormsAuth
{
    public class Global : HostedHttpApplication
    {
        protected override void ConfigureHost(HttpApplicationHostBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.RegisterWebJobActivator();

            builder.Services.AddSystemAdapters()
                .AddVirtualizedContentDirectories()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppServer(builder.Configuration.GetSection("RemoteApp").Bind)
                .AddAuthenticationServer();
        }

        protected override void Application_Start()
        {
            base.Application_Start();

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
