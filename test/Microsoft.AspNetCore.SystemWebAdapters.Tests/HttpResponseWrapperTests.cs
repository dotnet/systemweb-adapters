using System;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class HttpResponseWrapperTests
    {
        [Fact]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpResponseWrapper(null!));
        }

        [InlineData("/", "~", "/", true, null)]
        [InlineData("/", "~", "/", true, false)]
        [InlineData("/", "~", "/", true, true)]
        [InlineData("/", "~", "/", false, null)]
        [InlineData("/", "~", "/", false, false)]
        [InlineData("/", "~", "/", false, true)]

        [InlineData("/", "~/dir", "/dir", true, null)]
        [InlineData("/", "~/dir", "/dir", true, false)]
        [InlineData("/", "~/dir", "/dir", true, true)]
        [InlineData("/", "~/dir", "/dir", false, null)]
        [InlineData("/", "~/dir", "/dir", false, false)]
        [InlineData("/", "~/dir", "/dir", false, true)]

        [InlineData("/", "/dir", "/dir", true, null)]
        [InlineData("/", "/dir", "/dir", true, false)]
        [InlineData("/", "/dir", "/dir", true, true)]
        [InlineData("/", "/dir", "/dir", false, null)]
        [InlineData("/", "/dir", "/dir", false, false)]
        [InlineData("/", "/dir", "/dir", false, true)]

        [InlineData("/dir1/", "/dir2", "/dir2", true, null)]
        [InlineData("/dir1/", "/dir2", "/dir2", true, false)]
        [InlineData("/dir1/", "/dir2", "/dir2", true, true)]
        [InlineData("/dir1/", "/dir2", "/dir2", false, null)]
        [InlineData("/dir1/", "/dir2", "/dir2", false, false)]
        [InlineData("/dir1/", "/dir2", "/dir2", false, true)]

        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, null)]
        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, false)]
        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", true, true)]
        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, null)]
        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, false)]
        [InlineData("/dir1/", "~/dir2", "/dir1/dir2", false, true)]

        [InlineData("/dir1/", "", "/", true, null)]
        [InlineData("/dir1/", "", "/", true, false)]
        [InlineData("/dir1/", "", "/", true, true)]
        [InlineData("/dir1/", "", "/", false, null)]
        [InlineData("/dir1/", "", "/", false, false)]
        [InlineData("/dir1/", "", "/", false, true)]

        [Theory]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Testing")]
        public void Redirect(string vdir, string url, string resolved, bool permanent, bool? endResponse)
        {
            // Arrange
            var isEndCalled = endResponse ?? true;

            var options = new SystemWebAdaptersOptions
            {
                AppDomainAppVirtualPath = vdir,
            };

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<SystemWebAdaptersOptions>))).Returns(Options.Create(options));

            var endFeature = new Mock<IHttpResponseEndFeature>();
            endFeature.SetupAllProperties();

            var context = new DefaultHttpContext();
            context.Features.Set(endFeature.Object);
            context.Features.Set(new Mock<IHttpResponseContentFeature>().Object);
            context.RequestServices = services.Object;


            // Assemble: The HttpResponse and HttpResponseWrapper
            HttpResponse response = new HttpResponse(context.Response);
            HttpResponseBase responseBase = new HttpResponseWrapper(response);

            // Act: On the HttpResponseBase
            if (endResponse.HasValue)
            {
                if (permanent)
                {
                    responseBase.RedirectPermanent(url, endResponse.Value);
                }
                else
                {
                    responseBase.Redirect(url, endResponse.Value);
                }
            }
            else
            {
                if (permanent)
                {
                    responseBase.RedirectPermanent(url);
                }
                else
                {
                    responseBase.Redirect(url);
                }
            }

            // Assert: On the inner HttpResponse
            Assert.Equal(resolved, response.RedirectLocation);
            Assert.Null(context.Features.GetRequired<IHttpResponseFeature>().ReasonPhrase);
            Assert.Equal(2, context.Response.Headers.Count);
            Assert.Equal(resolved, context.Response.Headers.Location);
            Assert.Equal("text/html", context.Response.Headers.ContentType);
            Assert.Equal(permanent ? 301 : 302, context.Response.StatusCode);

            endFeature.Verify(b => b.EndAsync(), isEndCalled ? Times.Once : Times.Never);
        }
    }
}
