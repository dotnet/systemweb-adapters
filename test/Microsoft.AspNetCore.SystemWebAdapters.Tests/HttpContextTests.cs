// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters
{
    public class HttpContextTests
    {
        private readonly Fixture _fixture;

        public HttpContextTests()
        {
            _fixture = new Fixture();
        }

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
        public void UserIsProxiedWhenSetOnCoreAfterAdapter()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(coreContext.User, context.User);

            // Set a user to force the IPrincipalUserFeature to be used
            var user1 = new ClaimsPrincipal();
            context.User = user1;

            // Verify a new user sets things correctly if done from the ASP.NET Core side
            var user2 = new ClaimsPrincipal();
            coreContext.User = user2;

            Assert.Same(coreContext.User, user2);
            Assert.Same(context.User, user2);
        }

        [Fact]
        public void UserIsNotClaimsPrincipal()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(coreContext.User, context.User);

            var newUser = new Mock<IPrincipal>();
            context.User = newUser.Object;

            Assert.NotSame(coreContext.User, newUser.Object);
            Assert.Same(context.User, newUser.Object);
        }

        [Fact]
        public void UserIsDerivedClaimsPrincipal()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(coreContext.User, context.User);

            var newUser = new MyPrincipal();
            context.User = newUser;

            Assert.Same(coreContext.User, newUser);
        }

        private sealed class MyPrincipal : ClaimsPrincipal
        {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = Constants.CA1859)]
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

        [Fact]
        public void ErrorNoFeature()
        {
            // Arrange
            var context = new HttpContext(new DefaultHttpContext());

            // Assert
            var error = context.Error;
            var allErrors = context.AllErrors;

            // No ops
            context.ClearError();
            context.AddError(new InvalidOperationException());

            // Assert
            Assert.Null(error);
            Assert.Empty(allErrors);
        }

        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [Theory]
        public void ErrorWithFeatureGet(int length)
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            var exceptionFeature = new Mock<IRequestExceptionFeature>();
            var errors = new List<Exception>();

            for (var i = 0; i < length; i++)
            {
                errors.Add(new Mock<Exception>().Object);
            }

            exceptionFeature.Setup(f => f.Exceptions).Returns(errors);
            coreContext.Features.Set(exceptionFeature.Object);

            // Act
            var error = context.Error;
            var allErrors = context.AllErrors;

            // Assert
            Assert.Equal(allErrors, errors);
            Assert.NotSame(allErrors, errors);

            if (length > 0)
            {
                Assert.Same(error, errors[0]);
            }
            else
            {
                Assert.Null(error);
            }
        }

        [Fact]
        public void ErrorWithFeatureClear()
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            var exceptionFeature = new Mock<IRequestExceptionFeature>();
            coreContext.Features.Set(exceptionFeature.Object);

            // Act
            context.ClearError();

            // Assert
            exceptionFeature.Verify(f => f.Clear(), Times.Once);
        }

        [Fact]
        public void ErrorWithFeatureAdd()
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            var error = new InvalidOperationException();
            var exceptionFeature = new Mock<IRequestExceptionFeature>();
            coreContext.Features.Set(exceptionFeature.Object);

            // Act
            context.AddError(error);

            // Assert
            exceptionFeature.Verify(f => f.Add(error), Times.Once);
        }

        [InlineData("path1", "/path1", null)]
        [InlineData("/path1", "/path1", null)]
        [InlineData("path1?", "/path1", "")]
        [InlineData("/path1?", "/path1", "")]
        [InlineData("path1?q=1", "/path1", "?q=1")]
        [InlineData("/path1?q=1", "/path1", "?q=1")]
        [InlineData("/path1 ?q=1", "/path1", "?q=1")]
        [Theory]
        public void RewritePath(string rewritePath, string finalPath, string finalQuery)
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var feature = new Mock<IHttpRequestPathFeature>();
            coreContext.Features.Set(feature.Object);
            var context = new HttpContext(coreContext);

            // Act
            context.RewritePath(rewritePath);

            // Assert
            feature.Verify(f => f.Rewrite(finalPath, string.Empty, finalQuery, true), Times.Once);
        }

        [InlineData("path1", "/path1", null, true)]
        [InlineData("path1", "/path1", null, false)]
        [InlineData("/path1", "/path1", null, true)]
        [InlineData("/path1", "/path1", null, false)]
        [InlineData("path1?", "/path1", "", true)]
        [InlineData("path1?", "/path1", "", false)]
        [InlineData("/path1?", "/path1", "", true)]
        [InlineData("/path1?", "/path1", "", false)]
        [InlineData("path1?q=1", "/path1", "?q=1", true)]
        [InlineData("path1?q=1", "/path1", "?q=1", false)]
        [InlineData("/path1?q=1", "/path1", "?q=1", true)]
        [InlineData("/path1?q=1", "/path1", "?q=1", false)]
        [InlineData("/path1 ?q=1", "/path1", "?q=1", true)]
        [InlineData("/path1 ?q=1", "/path1", "?q=1", false)]
        [Theory]
        public void RewritePathWithRebaseValue(string rewritePath, string finalPath, string finalQuery, bool rebase)
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var feature = new Mock<IHttpRequestPathFeature>();
            coreContext.Features.Set(feature.Object);
            var context = new HttpContext(coreContext);

            // Act
            context.RewritePath(rewritePath, rebase);

            // Assert
            feature.Verify(f => f.Rewrite(finalPath, string.Empty, finalQuery, rebase), Times.Once);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public void RewritePathWithPathInfo(bool rebase)
        {
            // Arrange
            var coreContext = new DefaultHttpContext();
            var feature = new Mock<IHttpRequestPathFeature>();
            coreContext.Features.Set(feature.Object);
            var context = new HttpContext(coreContext);

            var filePath = _fixture.Create<string>();
            var pathInfo = _fixture.Create<string>();
            var query = _fixture.Create<string>();

            // Act
            context.RewritePath(filePath, pathInfo, query, rebase);

            // Assert
            feature.Verify(f => f.Rewrite(filePath, pathInfo, query, rebase), Times.Once);
        }
    }
}
