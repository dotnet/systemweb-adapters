using System;
using System.Web;
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
    }
}
