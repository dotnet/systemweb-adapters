using System;
using System.Configuration;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RemoteSessionFramework
{
    public class SessionApplication : HostedHttpApplication
    {
        protected override void ConfigureHost(HttpApplicationHostBuilder builder)
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
