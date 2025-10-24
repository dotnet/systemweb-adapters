using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MvcApp.Startup))]
namespace MvcApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/health", app2 =>
            {
                app2.Run(ctx =>
                {
                    return ctx.Response.WriteAsync("OK");
                });
            });
        }
    }
}
