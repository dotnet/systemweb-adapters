using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.AspNetCore.SystemWebAdapters.Authentication;

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
                .AddForwardedClaimsPrincipalSupport()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppSession(options=>
                {
                    options.ApiKey = ClassLibrary.SessionUtils.ApiKey;
                    ClassLibrary.SessionUtils.RegisterSessionKeys(options);
                });
        }
    }
}
