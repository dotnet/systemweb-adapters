// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters
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

            var httpRuntime = Microsoft.Extensions.DependencyInjection.HttpRuntimeFactory.Create(serviceProvider.Object);
            HttpRuntime.Current = httpRuntime;

            var coreContext = new Mock<HttpContextCore>();
            var context = new HttpContext(coreContext.Object);
            
            // Act
            var result = context.Cache;

            // Assert
        }

        [Fact]
        public void CacheFromHttpContextWrapper()
        {
            // Arrange
            var cache = new Cache();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(Cache))).Returns(cache);

            var httpRuntime = Microsoft.Extensions.DependencyInjection.HttpRuntimeFactory.Create(serviceProvider.Object);
            HttpRuntime.Current = httpRuntime;

            var coreContext = new Mock<HttpContextCore>();
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

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(Cache))).Returns(cache);

            var httpRuntime = Microsoft.Extensions.DependencyInjection.HttpRuntimeFactory.Create(serviceProvider.Object);
            HttpRuntime.Current = httpRuntime;

            // Act
            var result = System.Web.HttpRuntime.Cache;
            Assert.Same(cache, result);
        }
    }
}
