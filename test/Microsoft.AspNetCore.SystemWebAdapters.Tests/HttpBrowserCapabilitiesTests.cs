using System;
using System.Web;
using System.Web.Configuration;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class HttpBrowserCapabilitiesTests
    {
        [Fact]
        public void IsMobileDeviceCheck()
        {
            // Arrange
            string userAgent = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Act
                var _ = new HttpBrowserCapabilities(new BrowserCapabilitiesFactory(), userAgent);
            });
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 12_0_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.1", true)]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36", false)]
        public void IsMobileDevice(string userAgent, bool expected)
        {
            // Arrange
            HttpBrowserCapabilities httpBrowserCapabilities = new HttpBrowserCapabilities(new BrowserCapabilitiesFactory(), userAgent);

            // Act
            var actual = httpBrowserCapabilities.IsMobileDevice;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
