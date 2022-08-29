using System;
using System.Web;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Tests
{
    public class VirtualPathUtilityTests
    {
        [InlineData("/", "/")]
        [InlineData("~/", "/")]
        [InlineData("~/test/../other", "/other")]
        [Theory]
        public void ToAbsolute(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.ToAbsolute(virtualPath, "/"));
        }

        [Fact]
        public void ToAbsoluteError()
        {
            // This does not match the documentation
            // https://docs.microsoft.com/en-us/dotnet/api/system.web.virtualpathutility.toabsolute?view=netframework-4.8
            // but it does match the actual framework
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.ToAbsolute("hello", "/"));
			Assert.Throws<ArgumentException>(() => VirtualPathUtility.ToAbsolute("../../test", "/"));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.ToAbsolute("~hello", "/"));
            Assert.Throws<HttpException>(() => VirtualPathUtility.ToAbsolute("~/../../test", "/"));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.ToAbsolute("~/hello", null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.ToAbsolute("~/hello", ""));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.ToAbsolute("~/hello", "world"));
        }

        [InlineData("/", "~/")]
        [InlineData("~/", "~/")]
        [Theory]
        public void ToAppRelative(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.ToAppRelative(virtualPath, "/"));
        }


        [InlineData("~/test", "hello", "~/hello")]
        [InlineData("~/test/world", "hello", "~/test/hello")]
        [InlineData("~/test/world", "../hello", "~/hello")]
        [Theory]
        public void Combine(string basePath, string relativePath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.Combine(basePath, relativePath));
        }

        [Fact]
        public void CombineError()
        {
            Assert.Throws<System.Web.HttpException>(() => VirtualPathUtility.Combine("~/", "../../"));
        }

        [InlineData("", "")]        // This isn't mentioned in the docs but matches the behaviour of ASP.NET 4.x
        [InlineData(null, null)]      // This isn't mentioned in the docs but matches the behaviour of ASP.NET 4.x
        [InlineData("/", "/")]
        [InlineData("/test", "/test/")]
        [InlineData("test", "test/")]
        [Theory]
        public void AppendTrailingSlash(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.AppendTrailingSlash(virtualPath));
        }

        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("/", "/")]
        [InlineData("/test/", "/test")]
        [Theory]
        public void RemoveTrailingSlash(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.RemoveTrailingSlash(virtualPath));
        }

        // These are conditional so that these tests can be run against net48.
        // The GetDirectory function doesn't work unless for app relative paths unless running as a web application.
#if NET6_0_OR_GREATER
        [InlineData("~", "/")]
        [InlineData("~/", "/")]
        [InlineData("~/test", "~/")]
        [InlineData("~/test/world.jpg", "~/test/")]
#endif
        [InlineData("/", null)]
        [InlineData("/test", "/")]
        [InlineData("/test/world.jpg", "/test/")]
        [Theory]
        public void GetDirectory(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.GetDirectory(virtualPath));
        }

        [Fact]
        public void GetDirectoryError()
        {
			Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetDirectory(null!));
			Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetDirectory(""));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.GetDirectory("test"));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.GetDirectory("test/world.jpg"));
        }

        [InlineData("/", "")]
        [InlineData("/test", "")]
        [InlineData("test", "")]
        [InlineData("/test/world.jpg", ".jpg")]
        [InlineData("test/world.jpg", ".jpg")]
        [Theory]
        public void GetExtension(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.GetExtension(virtualPath));
        }

        [Fact]
        public void GetExtensionError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetExtension(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetExtension(""));
        }

        [InlineData("/", "")]
        [InlineData("/test", "test")]
        [InlineData("/test/world.jpg", "world.jpg")]
        [InlineData("~/", "")]
        [InlineData("~/test", "test")]
        [InlineData("~/test/world.jpg", "world.jpg")]
        [Theory]
        public void GetFileName(string virtualPath, string expected)
        {
            Assert.Equal(expected, (string?)VirtualPathUtility.GetFileName(virtualPath));
        }

        [Fact]
        public void GetFileNameError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetFileName(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.GetFileName(""));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.GetFileName("test"));
            Assert.Throws<ArgumentException>(() => VirtualPathUtility.GetFileName("test/world.jpg"));
        }

        [InlineData("~", false)]
        [InlineData("/", true)]
        [InlineData("~/", false)]
        [InlineData("/test", true)]
        [InlineData("test", false)]
        [InlineData("/test/world.jpg", true)]
        [InlineData("test/world.jpg", false)]
        [InlineData("~/test", false)]
        [InlineData("~/test/world.jpg", false)]
        [Theory]
        public void IsAbsolute(string virtualPath, bool expected)
        {
            Assert.Equal(expected, VirtualPathUtility.IsAbsolute(virtualPath));
        }

        [Fact]
        public void IsAbsoluteError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.IsAbsolute(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.IsAbsolute(""));
        }

        [InlineData("~", true)]
        [InlineData("/", false)]
        [InlineData("~/", true)]
        [InlineData("/test", false)]
        [InlineData("test", false)]
        [InlineData("/test/world.jpg", false)]
        [InlineData("test/world.jpg", false)]
        [InlineData("~/test", true)]
        [InlineData("~/test/world.jpg", true)]
        [Theory]
        public void IsAppRelative(string virtualPath, bool expected)
        {
            Assert.Equal(expected, VirtualPathUtility.IsAppRelative(virtualPath));
        }

        [Fact]
        public void IsAppRelativeError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.IsAppRelative(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtility.IsAppRelative(""));
        }

        // These are conditional so that these tests can be run against net48.
        // The MakeRelative function doesn't work unless for app relative paths unless running as a web application.
#if NET6_0_OR_GREATER
        [InlineData("~/home/index", "~/api/weather/get", "../api/weather/get")]
        [InlineData("~/home/index", "~/other/index", "../other/index")]
        [InlineData("~/home/index", "~/home/other", "other")]
        [InlineData("~/home/index", "/api/weather/get", "../api/weather/get")]
        [InlineData("/home/index", "~/other/index", "../other/index")]
        [InlineData("", "~/", "./")]
        [InlineData("~/", "", "")]
        [InlineData("", "/", "./")]
        [InlineData("/", "", "")]
#endif
        [InlineData("/home/index", "/api/weather/get", "../api/weather/get")]
        [InlineData("/home/index", "/other/index", "../other/index")]
        [InlineData("/home/index", "/home/other", "other")]
        [InlineData("/directory1/file1.aspx", "/directory2/file2.aspx", "../directory2/file2.aspx")]
        [Theory]
        public void MakeRelative(string fromPath, string toPath, string expected)
        {
            Assert.Equal(expected, VirtualPathUtility.MakeRelative(fromPath, toPath));
        }

        [Fact]
        public void MakeRelativeError()
        {
            Assert.Throws<NullReferenceException>(() => VirtualPathUtility.MakeRelative("~/", null!));
            Assert.Throws<NullReferenceException>(() => VirtualPathUtility.MakeRelative(null!, "~/"));
            Assert.Throws<ArgumentOutOfRangeException>(() => VirtualPathUtility.MakeRelative("~/hello/", "test"));
            Assert.Throws<ArgumentOutOfRangeException>(() => VirtualPathUtility.MakeRelative("test", "~/hello/"));
        }
    }
}
