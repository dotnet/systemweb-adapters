using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebFormsFramework
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddVirtualizedContentDirectories()
                .AddProxySupport(options => options.UseForwardedHeaders = true);

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
