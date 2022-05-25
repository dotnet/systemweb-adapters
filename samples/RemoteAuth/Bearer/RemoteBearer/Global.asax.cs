using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace RemoteOAuth
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Application.AddSystemWebAdapters()
                .AddRemoteAuthentication(options =>
                {
                    // A real application would not hard code this, but load it
                    // securely from environment or configuration
                    options.RemoteServiceOptions.ApiKey = "TopSecretString";
                });
        }
    }
}
