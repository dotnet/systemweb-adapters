using System.Web.Http;
using System.Web.Mvc;
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
                .AddRemoteApp(options =>
                {
                    // A real application would not hard code this, but load it
                    // securely from environment or configuration
                    options.ApiKey = "TopSecretString";
                })
                .AddRemoteAppAuthentication();
        }
    }
}
