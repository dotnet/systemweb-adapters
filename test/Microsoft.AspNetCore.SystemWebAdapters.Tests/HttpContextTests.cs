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
    public class HttpContextTests
    {
        [Fact]
        public void ConstructorChecksNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpContext(null!));
        }

        [Fact]
        public void RequestIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Request, context.Request);
        }

        [Fact]
        public void ResponseIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Response, context.Response);
        }

        [Fact]
        public void ServerIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Server, context.Server);
        }

        [Fact]
        public void UserIsProxied()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(coreContext.User, context.User);

            var newUser = new ClaimsPrincipal();
            context.User = newUser;

            Assert.Same(coreContext.User, newUser);
        }

        [Fact]
        public void NonClaimsPrincipalIsCopied()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            var newUser = new Mock<IPrincipal>();
            context.User = newUser.Object;

            Assert.NotSame(coreContext.User, newUser);
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Use for tests")]
        public void GetServiceReturnsExpected()
        {
            var coreContext = new DefaultHttpContext();
            coreContext.Features.Set(new HttpSessionState(new Mock<ISessionState>().Object));

            var context = new HttpContext(coreContext);
            var provider = (IServiceProvider)context;

            Assert.Same(context.Request, provider.GetService(typeof(HttpRequest)));
            Assert.Same(context.Response, provider.GetService(typeof(HttpResponse)));
            Assert.Same(context.Server, provider.GetService(typeof(HttpServerUtility)));
            Assert.Same(context.Session, provider.GetService(typeof(HttpSessionState)));

            Assert.Null(provider.GetService(typeof(HttpContext)));
        }

        [Fact]
        public void DefaultItemsContains()
        {
            // Arrange
            var key = new object();
            var value = new object();
            var items = new Mock<IDictionary<object, object?>>();
            items.Setup(i => i[key]).Returns(value);
            items.Setup(i => i.TryGetValue(key, out value)).Returns(true);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Items).Returns(items.Object);

            var context = new HttpContext(coreContext.Object);

            // Act
            var result = context.Items[key];

            // Assert
            Assert.Same(value, result);
        }

        [Fact]
        public void ItemsNotWrappedIfAlreadyImplementsIDictionary()
        {
            // Arrange
            // Use Dictionary<TKey, TValue> since it implements the non-generic IDictionary
            var items = new Dictionary<object, object?>();
            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Items).Returns(items);

            var context = new HttpContext(coreContext.Object);

            // Act
            var result = context.Items;

            // Assert
            Assert.Same(items, result);
        }

        [Fact]
        public void CacheFromServices()
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
        public void DisposeOnPipelineCompleted()
        {
            // Arrange
            var coreContext = new Mock<HttpContextCore>();
            var coreResponse = new Mock<HttpResponseCore>();

            coreContext.Setup(c => c.Response).Returns(coreResponse.Object);

            var context = new HttpContext(coreContext.Object);
            var disposable = new Mock<IDisposable>();

            // Act
            var token = context.DisposeOnPipelineCompleted(disposable.Object);

            // Assert
            Assert.True(token.IsActive);
        }

        [Fact]
        public void DisposeOnPipelineCompletedUnsubscribed()
        {
            // Arrange
            var coreContext = new Mock<HttpContextCore>();
            var coreResponse = new Mock<HttpResponseCore>();

            coreContext.Setup(c => c.Response).Returns(coreResponse.Object);

            var context = new HttpContext(coreContext.Object);
            var disposable = new Mock<IDisposable>();

            // Act
            var token = context.DisposeOnPipelineCompleted(disposable.Object);

            token.Unsubscribe();

            // Assert
            Assert.False(token.IsActive);
        }

        [Fact]
        public void DisposeOnPipelineCompletedUnsubscribedDisposed()
        {
            // Arrange
            IDisposable registeredDisposable = null!;

            var coreContext = new Mock<HttpContextCore>();
            var coreResponse = new Mock<HttpResponseCore>();
            coreResponse.Setup(c => c.RegisterForDispose(It.IsAny<IDisposable>()))
                .Callback((IDisposable disposable) => registeredDisposable = disposable);

            coreContext.Setup(c => c.Response).Returns(coreResponse.Object);

            var context = new HttpContext(coreContext.Object);
            var disposable = new Mock<IDisposable>();

            // Act
            var token = context.DisposeOnPipelineCompleted(disposable.Object);
            token.Unsubscribe();
            registeredDisposable.Dispose();

            // Assert
            Assert.False(token.IsActive);
            disposable.Verify(d => d.Dispose(), Times.Never);
        }

        [Fact]
        public void DisposeOnPipelineCompletedDisposed()
        {
            // Arrange
            var coreContext = new Mock<HttpContextCore>();
            var coreResponse = new Mock<HttpResponseCore>();
            coreResponse.Setup(c => c.RegisterForDispose(It.IsAny<IDisposable>()))
                .Callback((IDisposable disposable) => disposable.Dispose());

            coreContext.Setup(c => c.Response).Returns(coreResponse.Object);

            var context = new HttpContext(coreContext.Object);
            var disposable = new Mock<IDisposable>();

            // Act
            var token = context.DisposeOnPipelineCompleted(disposable.Object);

            // Assert
            Assert.False(token.IsActive);
            disposable.Verify(d => d.Dispose(), Times.Once);
        }

        [InlineData("path1", "/path1", "", 0)]
        [InlineData("/path1", "/path1", "", 0)]
        [InlineData("path1?", "/path1", "", 0)]
        [InlineData("/path1?", "/path1", "", 0)]
        [InlineData("path1?q=1", "/path1", "?q=1", 1)]
        [InlineData("/path1?q=1", "/path1", "?q=1", 1)]
        [InlineData("/path1 ?q=1", "/path1", "?q=1", 1)]
        [Theory]
        public void RewritePath(string rewritePath, string finalPath, string finalQuery, int queryCount)
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            // Act
            context.RewritePath(rewritePath);

            // Assert
            Assert.Equal(finalPath, coreContext.Request.Path);
            Assert.Equal(new(finalQuery), coreContext.Request.QueryString);
            Assert.Equal(queryCount, coreContext.Request.Query.Count);
        }

        [Fact]
        public void RewritePathWithSpace()
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);
            var rewritePath = "/path1 withspace?q=1";

            // Act
            context.RewritePath(rewritePath);

            // Assert
            Assert.Equal("/path1%20withspace", coreContext.Request.Path);
            Assert.Equal("/path1 withspace", context.Request.Path);
            Assert.Collection(coreContext.Request.Query, q =>
            {
                Assert.Equal("q", q.Key);
                Assert.Equal("1", q.Value);
            });
        }
    }
}
