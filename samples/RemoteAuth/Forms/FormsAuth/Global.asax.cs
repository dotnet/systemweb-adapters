using System;
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
                .AddRemoteAppServer(remote => remote
                    .Configure(options =>
                    {
                        // Do not re-use this ApiKey; every solution should use a unique ApiKey
                        options.ApiKey = "8e470586-24e5-4f2a-8245-69bbdbf9f767";
                    })
                    .AddAuthentication());
        }
    }
}
