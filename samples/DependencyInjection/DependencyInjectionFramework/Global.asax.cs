using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
                builder.AddSystemWebDependencyInjection();

                builder.Services.AddSingleton<SingletonService>(SingletonService.Instance);
                builder.Services.AddScoped<ScopedService>();
                builder.Services.AddTransient<TransientService>();
            });

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }

    public class SingletonService
    {
        public static SingletonService Instance { get; } = new SingletonService();
    }

    public class TransientService
    {
    }

    public class ScopedService
    {
    }

    public class TestService
    {
        public static bool IsValid(SingletonService singleton, TransientService transient1, TransientService transient2)
        {
            if (!ReferenceEquals(SingletonService.Instance, singleton))
            {
                return false;
            }

            if (ReferenceEquals(transient1, transient2))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(SingletonService singleton, ScopedService scoped1, ScopedService scoped2, TransientService transient1, TransientService transient2)
        {
            IsValid(singleton, transient1, transient2);

            if (!ReferenceEquals(scoped1, scoped2))
            {
                return false;
            }

            using (var testScope = HttpRuntime.WebObjectActivator.CreateScope())
            {
                var scopedInNewScope = testScope.ServiceProvider.GetRequiredService<ScopedService>();
                if (ReferenceEquals(scoped1, scopedInNewScope))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

