using System;
using System.Configuration;
using System.Web;
using System.Web.Routing;

namespace RemoteSessionFramework
{
    public class SessionApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SystemWebAdapterConfiguration.AddSystemWebAdapters(this)
                .AddProxySupport(options => options.UseForwardedHeaders = true)
                .AddSessionSerializer(options =>
                {
                })
                .AddJsonSessionSerializer(options =>
                {
                    options.RegisterKey<int>("CoreCount");
                })
                .AddRemoteAppServer(options => options.ApiKey = ConfigurationManager.AppSettings["RemoteApp__ApiKey"])
                .AddSessionServer(options =>
                {
                });
        }

        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Session is { } session)
            {
                if (session["FrameworkCount"] is int count)
                {
                    session["FrameworkCount"] = count + 1;
                }
                else
                {
                    session["FrameworkCount"] = 1;
                }
            }
        }
    }
}
