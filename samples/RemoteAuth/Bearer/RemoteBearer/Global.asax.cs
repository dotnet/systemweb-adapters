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
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            Application.AddSystemWebAdapters()
                .AddRemoteAppAuthentication(options =>
                {
                    // A real application would not hard code this, but load it
                    // securely from environment or configuration
                    options.RemoteServiceOptions.ApiKey = "TopSecretString";
                });
        }
    }
}
