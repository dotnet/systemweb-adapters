// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

public class ConditionalEndpointTests
{
    [Theory]
    [InlineData("?", false)]
    [InlineData("?skip=true", true)]
    public async Task VerifyEndpointsAsync(string queryString, bool isSkipped)
    {
        const string EndpointPath = "/";

        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddConditionalEndpoints<TestSkippable>();

        var app = builder.Build();
        var marker = new Marker();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.Map(EndpointPath, () => "response1")
                .WithMetadata(marker)
                .WithConditionalRoute();
        });

        var request = ((IApplicationBuilder)app).Build();
        var context = new DefaultHttpContext();
        context.Request.Path = EndpointPath;
        context.Request.QueryString = new QueryString(queryString);

        // Act
        await request(context);

        // Assert
        var endpoint = context.GetEndpoint();

        if (isSkipped)
        {
            Assert.Equal(404, context.Response.StatusCode);
            Assert.Null(endpoint);
        }
        else
        {
            Assert.Equal(200, context.Response.StatusCode);
            Assert.NotNull(endpoint);
            Assert.Single(endpoint.Metadata.GetOrderedMetadata<Marker>());
        }
    }

    private sealed class Marker
    {
    }

    private sealed class TestSkippable : IConditionalEndpointSelector
    {
        public ValueTask<bool> IsEnabledAsync(HttpContextCore context, Endpoint candidate)
        {
            var result = context.Request.Query.ContainsKey("skip");

            return ValueTask.FromResult(result);
        }
    }
}
