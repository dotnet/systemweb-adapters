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
            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddVirtualizedContentDirectories()
                .AddRemoteAppServer(options => options.ApiKey = ConfigurationManager.AppSettings["RemoteAppApiKey"])
                .AddAuthenticationServer();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }
}
