// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET7_0_OR_GREATER

using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

[Collection(nameof(SelfHostedTests))]
public class OutputCachingTests
{
    [Fact]
    public async Task OutputCachingWorksWithHttpApplication()
    {
        // Arrange
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
                        .AddHttpApplication<MyApp>();
                      services.AddOutputCache();
                  })
                  .Configure(app =>
                  {
                      app.UseRouting();
                      app.UseSystemWebAdapters();
                      app.UseOutputCache();
                      app.UseEndpoints(endpoints =>
                      {
                          var count = 0;
                          endpoints.Map("/", () => Results.Ok(count++))
                          .CacheOutput(builder =>
                          {
                              builder.AddHttpApplicationVaryByCustom(MyApp.VaryBy);
                          });
                      });
                  });
          })
          .StartAsync();

        using var client = host.GetTestClient();

        // Act
        var result1 = await GetResponseAsync(client, "test");
        var result2 = await GetResponseAsync(client, "other");
        var result3 = await GetResponseAsync(client, "test");

        // Assert
        Assert.Equal("0", result1);
        Assert.Equal("1", result2);
        Assert.Equal("0", result3);

        static async Task<string> GetResponseAsync(HttpClient client, string value)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, new Uri("/", UriKind.Relative))
            {
                Headers =
                {
                    { MyApp.Header, value }
                }
            };

            using var response = await client.SendAsync(message);

            return await response.Content.ReadAsStringAsync();
        }
    }

    private sealed class MyApp : HttpApplication
    {
        public const string VaryBy = "myValue";
        public const string Header = "X-TEST-HEADER";

        public override string? GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom == VaryBy)
            {
                return context.Request.Headers[Header];
            }

            return base.GetVaryByCustomString(context, custom);
        }
    }
}
#endif
