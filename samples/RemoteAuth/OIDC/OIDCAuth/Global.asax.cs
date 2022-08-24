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
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppServer(remote => remote
                    // Do not re-use this ApiKey; every solution should use a unique ApiKey
                    .Configure(options => options.ApiKey = "121257f2-c121-4f51-b30c-d1f617933290")
                    .AddAuthentication());

        }
    }
}
