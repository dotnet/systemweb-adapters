using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace RemoteOAuth
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddRemoteAppServer(remote => remote
                    .Configure(options =>
                    {
                        // A real application would not hard code this, but load it
                        // securely from environment or configuration
                        // Do not re-use this ApiKey; every solution should use a unique ApiKey
                        options.ApiKey = "40c807bd-6c00-4e5a-9650-ea20c2e6c02d";
                    })
                    .AddAuthentication());
        }
    }
}
