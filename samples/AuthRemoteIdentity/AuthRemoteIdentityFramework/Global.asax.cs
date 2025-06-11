using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Hosting;

namespace MvcApp
{
    public class MvcApplication : HostedHttpApplication
    {
        protected override void ConfigureHost(HttpApplicationHostBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.RegisterWebJobActivator();

            builder.Services.AddSystemAdapters()
                .AddVirtualizedContentDirectories()
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddRemoteAppServer(options => options.ApiKey = ConfigurationManager.AppSettings["RemoteApp__ApiKey"])
                .AddAuthenticationServer();
        }

        protected override void Application_Start()
        {
            base.Application_Start();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
