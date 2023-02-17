// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
using Autofac.Extras.Moq;
using Moq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class PreBufferRequestStreamMiddlewareTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task RequestBuffering(bool isDisabled)
    {
        // Arrange
        var next = new Mock<RequestDelegate>();
        using var mock = AutoMock.GetLoose(c => c.RegisterMock(next));

        var logger = new Mock<ILogger<PreBufferRequestStreamMiddleware>>();

        var metadata = new PreBufferRequestStreamAttribute { IsDisabled = isDisabled };

        var metadataCollection = new EndpointMetadataCollection(metadata);

        var endpointFeature = new Mock<IEndpointFeature>();
        endpointFeature.Setup(e => e.Endpoint).Returns(new Endpoint(null, metadataCollection, null));

        var stream = new Mock<Stream>();

        var requestFeature = new Mock<IHttpRequestFeature>();
        requestFeature.SetupProperty(r => r.Body);
        requestFeature.Object.Body = stream.Object;

        var responseFeature = new Mock<IHttpResponseFeature>();

        var features = new FeatureCollection();
        features.Set(endpointFeature.Object);
        features.Set(requestFeature.Object);
        features.Set(responseFeature.Object);
        features.Set(new Mock<IHttpResponseBodyFeature>().Object);

        var context = new DefaultHttpContext(features);

        // Act
        await mock.Create<RegisterAdapterFeaturesMiddleware>().InvokeAsync(context);
        await mock.Create<PreBufferRequestStreamMiddleware>().InvokeAsync(context);

        // Assert
        Assert.Equal(!isDisabled, context.Request.Body.CanSeek);
    }
}
