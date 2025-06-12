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
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
            {
                builder.AddServiceDefaults();
                builder.RegisterWebObjectActivator();

                builder.Services.AddSystemAdapters()
                    .AddVirtualizedContentDirectories()
                    .AddProxySupport(options => options.UseForwardedHeaders = true)
                    .AddRemoteAppServer(builder.Configuration.GetSection("RemoteApp").Bind)
                    .AddAuthenticationServer();
            });

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
