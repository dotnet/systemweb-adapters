using System.Configuration;
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
                        options.ApiKey = ConfigurationManager.AppSettings["RemoteAppApiKey"];
                    })
                    .AddAuthentication());
        }
    }
}
