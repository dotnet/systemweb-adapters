
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using global::Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ModulesLibrary;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests.SessionState
{
    [Collection(nameof(SelfHostedTests))]
    public class SetSessionStateBehaviorTests
    {
        [Fact]
        public async Task TestSetSessionStateBehavior()
        {
           
            using var host = await new HostBuilder()
              .ConfigureWebHost(webBuilder =>
              {
                  webBuilder
                      .UseTestServer(options =>
                      {
                          options.AllowSynchronousIO = true;
                      })
                      .ConfigureServices(services =>
                      {
                          services.AddRouting();
                          services.AddSystemWebAdapters()
                            .WrapAspNetCoreSession()
                            .AddHttpApplication<MyApp>(options =>
                            {
                                // Register a module by name
                                options.RegisterModule<MyModule>("MyModule");
                            });
                          services.AddDistributedMemoryCache();
                      })
                      .Configure(app =>
                      {
                          app.UseRouting();
                          app.UseSession();
                          app.UseAuthenticationEvents();
                          app.UseAuthorizationEvents();
                          app.UseSystemWebAdapters();
                          app.UseEndpoints(endpoints =>
                          {
                              endpoints.Map("/{*url}", (context) => MyController(context));
                              
                          });
                      });
              })
              .StartAsync();

            using var client = host.GetTestClient();

            // Act
            var result1 = await GetResponseAsync(client, "/disabled");
            var result2 = await GetResponseAsync(client, "/readonly");
            var result3 = await GetResponseAsync(client, "/required");

            // Assert
            Assert.Equal("Session:null", result1);
            Assert.Equal("ReadOnly:True", result2);
            Assert.Equal("ReadOnly:False", result3);

        }


        static async Task MyController(HttpContext context)
        {
            HttpSessionState? session = context.Session;
            if (session == null)
            {
                context.Response.Write("Session:null");
            } else
            {

                context.Response.Write($"ReadOnly:{session.IsReadOnly}"); ;
            }
        }
        static async Task<string> GetResponseAsync(HttpClient client, string uri)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, UriKind.Relative));
            using var response = await client.SendAsync(message);

            return await response.Content.ReadAsStringAsync();
        }

        internal sealed class MyApp : HttpApplication
        {
        }

        internal sealed class MyModule : IHttpModule
        {
            public void Dispose()
            {
            }

            public void Init(HttpApplication context)
            {
                // Below is an example of how you can handle LogRequest event and provide custom logging implementation
                context.BeginRequest += new EventHandler(BeginRequestHandler);
            }

            void BeginRequestHandler(object sender, EventArgs e)
            {
                // Custom logic goes here
                HttpApplication application = (HttpApplication)sender;
                HttpContext context = application.Context;
                string path = context.Request.Path;
                switch (path)
                {
                    case "/disabled":
                        context.SetSessionStateBehavior(SessionStateBehavior.Disabled); break;
                    case "/readonly":
                        context.SetSessionStateBehavior(SessionStateBehavior.ReadOnly); break;
                    case "/required":
                        context.SetSessionStateBehavior(SessionStateBehavior.Required); break;
                    case "/default":
                        context.SetSessionStateBehavior(SessionStateBehavior.Default); break;
                    default:
                        break;
                }
            }
        }
    }
}
