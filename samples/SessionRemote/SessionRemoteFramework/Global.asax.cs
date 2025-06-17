using System;
using System.Configuration;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RemoteSessionFramework
{
    public class SessionApplication : HttpApplication
    {
        protected void Application_Start()
        {
            HttpApplicationHost.RegisterHost(builder =>
            {
                builder.AddServiceDefaults();
                builder.Services.AddSystemAdapters()
                    .AddProxySupport(options => options.UseForwardedHeaders = true)
                    .AddSessionSerializer(options =>
                    {
                    })
                    .AddJsonSessionSerializer(options =>
                    {
                        options.RegisterKey<int>("CoreCount");
                    })
                    .AddRemoteAppServer(builder.Configuration.GetSection("RemoteApp").Bind)
                    .AddSessionServer();
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
