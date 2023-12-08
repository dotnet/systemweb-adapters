// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using System.Web.Caching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class CacheTests
    {

        [Fact]
        public void CacheFromHttpContext()
        {
            // Arrange
            var cache = new Cache();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(Cache))).Returns(cache);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);

            var context = new HttpContext(coreContext.Object);

            // Act
            var result = context.Cache;

            // Assert
            Assert.Same(cache, result);

            //Act via GetService
            var cacheFromService = context.GetService<Cache>();
            Assert.Same(cache, cacheFromService);
        }

        [Fact]
        public void CacheFromHttpContextWrapper()
        {
            // Arrange
            var cache = new Cache();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(Cache))).Returns(cache);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);

            var context = new HttpContext(coreContext.Object);
            var contextWrapper = new HttpContextWrapper(context);

            // Act
            var result = contextWrapper.Cache;

            // Assert
            Assert.Same(cache, result);

            //Act via GetService
            var cacheFromService = contextWrapper.GetService<Cache>();
            Assert.Same(cache, cacheFromService);
        }
    }
}
