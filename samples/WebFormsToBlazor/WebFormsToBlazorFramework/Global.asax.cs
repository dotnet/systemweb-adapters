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
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
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
            });

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
