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
        }

        [Fact]
        public void CacheFromHttpRuntime()
        {
            // Arrange
            var cache = new Cache();

            var httpRuntime = new Mock<IHttpRuntime>();
            httpRuntime.Setup(c=>c.Cache).Returns(cache);

            HttpRuntime.Current = httpRuntime.Object;

            // Act
            var result = System.Web.HttpRuntime.Cache;

            // Assert
            Assert.Same(cache, result);
        }

        [Fact]
        public void CacheFromHttpRuntimeFactory()
        {
            // Arrange
            var cache = new Cache();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(Cache))).Returns(cache);

            var httpRuntime = HttpRuntimeFactory.Create(serviceProvider.Object);
            HttpRuntime.Current = httpRuntime;

            // Act
            var result = System.Web.HttpRuntime.Cache;

            // Assert
            Assert.Same(cache, result);
        }
    }
}
