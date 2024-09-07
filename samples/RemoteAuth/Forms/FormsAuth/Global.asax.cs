using System;
using System.Configuration;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace FormsAuth
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppServer(options =>
                    {
                        options.ApiKey = ConfigurationManager.AppSettings["RemoteAppApiKey"];
                    })
                .AddAuthenticationServer();
        }
    }
}
