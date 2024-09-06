using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OIDCAuth
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddVirtualizedContentDirectories()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppServer(options => options.ApiKey = ConfigurationManager.AppSettings["RemoteAppApiKey"])
                .AddAuthenticationServer();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
