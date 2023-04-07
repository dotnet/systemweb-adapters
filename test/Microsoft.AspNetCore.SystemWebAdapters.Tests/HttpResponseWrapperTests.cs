using System;
using System.Web;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class HttpServerUtilityWrapperTests
    {
        [Fact]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpServerUtilityWrapper(null!));
        } 
    }
}
