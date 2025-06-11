using System;
using System.Configuration;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WebFormsFramework
{
    public class Global : HostedHttpApplication
    {
        protected override void ConfigureHost(HttpApplicationHostBuilder builder)
        {
            builder.AddServiceDefaults();

            builder.Services.AddSystemAdapters()
                .AddVirtualizedContentDirectories()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddJsonSessionSerializer(options =>
                {
                    options.RegisterKey<string>("test-value");
                })
                .AddRemoteAppServer(builder.Configuration.GetSection("RemoteApp").Bind)
                .AddSessionServer();
        }

        protected override void Application_Start()
        {
            base.Application_Start();

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
