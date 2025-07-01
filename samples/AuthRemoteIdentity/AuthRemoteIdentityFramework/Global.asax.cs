using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MvcApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
            {
                builder.AddServiceDefaults();
                builder.RegisterWebObjectActivator();

                builder.AddSystemWebAdapters()
                    .AddVirtualizedContentDirectories();
            });

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
