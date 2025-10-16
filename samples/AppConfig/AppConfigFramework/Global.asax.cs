using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Hosting;

namespace AppConfigFramework
{
    public class AppConfigApplication : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
            {
                builder.AddServiceDefaults();
                builder.RegisterWebObjectActivator();
            });
        }
    }
}
