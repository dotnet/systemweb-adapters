using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;

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

            Application.AddSystemWebAdapters()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppSession(
                    ConfigureRemoteServiceOptions,
                    options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options))
                .AddRemoteAppAuthentication(o => ConfigureRemoteServiceOptions(o.RemoteServiceOptions));
        }

        private void ConfigureRemoteServiceOptions(RemoteServiceOptions options)
        {
            options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
        }
    }
}
