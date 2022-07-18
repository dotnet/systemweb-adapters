using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MvcApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            this.AddSystemWebAdapters()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteApp(options =>
                {
                    options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
                })
                .AddRemoteAppSession()
                .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys))
                .AddRemoteAppAuthentication();
        }
    }
}
