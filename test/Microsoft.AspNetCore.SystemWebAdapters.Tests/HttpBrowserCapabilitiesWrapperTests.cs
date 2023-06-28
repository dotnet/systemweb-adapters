using System;
using System.Web;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class HttpBrowserCapabilitiesWrapperTests
    {
        [Fact]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpBrowserCapabilitiesWrapper(null!));
        } 
    }
}
