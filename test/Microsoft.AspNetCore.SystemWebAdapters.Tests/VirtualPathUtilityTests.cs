// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web;
using Moq;
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
            var virtualPathUtility = CreateUtility();

            Assert.Equal(expected, virtualPathUtility.ToAbsolute(virtualPath, "/"));
        }

        [Fact]
        public void ToAbsoluteError()
        {
            var virtualPathUtility = CreateUtility();

            // This does not match the documentation
            // https://docs.microsoft.com/en-us/dotnet/api/system.web._utility.toabsolute?view=netframework-4.8
            // but it does match the actual framework
            Assert.Throws<ArgumentException>(() => virtualPathUtility.ToAbsolute("hello", "/"));
            Assert.Throws<ArgumentException>(() => virtualPathUtility.ToAbsolute("../../test", "/"));
            Assert.Throws<ArgumentException>(() => virtualPathUtility.ToAbsolute("~hello", "/"));
            Assert.Throws<HttpException>(() => virtualPathUtility.ToAbsolute("~/../../test", "/"));
            Assert.Throws<ArgumentNullException>(() => virtualPathUtility.ToAbsolute("~/hello", null!));
            Assert.Throws<ArgumentNullException>(() => virtualPathUtility.ToAbsolute("~/hello", ""));
            Assert.Throws<ArgumentException>(() => virtualPathUtility.ToAbsolute("~/hello", "world"));
        }

        [InlineData("/", "~/")]
        [InlineData("~/", "~/")]
        [Theory]
        public void ToAppRelative(string virtualPath, string expected)
        {
            Assert.Equal(expected, VirtualPathUtilityImpl.ToAppRelative(virtualPath, "/"));
        }

        [InlineData("~/test", "hello", "~/hello")]
        [InlineData("~/test/world", "hello", "~/test/hello")]
        [InlineData("~/test/world", "../hello", "~/hello")]
        [Theory]
        public void Combine(string basePath, string relativePath, string expected)
        {
            var virtualPathUtility = CreateUtility();

            Assert.Equal(expected, virtualPathUtility.Combine(basePath, relativePath));
        }

        [Fact]
        public void CombineError()
        {
            var virtualPathUtility = CreateUtility();

            Assert.Throws<HttpException>(() => virtualPathUtility.Combine("~/", "../../"));
        }

        [InlineData("", "")]        // This isn't mentioned in the docs but matches the behaviour of ASP.NET 4.x
        [InlineData(null, null)]      // This isn't mentioned in the docs but matches the behaviour of ASP.NET 4.x
        [InlineData("/", "/")]
        [InlineData("/test", "/test/")]
        [InlineData("test", "test/")]
        [Theory]
        public void AppendTrailingSlash(string virtualPath, string expected)
        {
            Assert.Equal(expected, VirtualPathUtilityImpl.AppendTrailingSlash(virtualPath));
        }

        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("/", "/")]
        [InlineData("/test/", "/test")]
        [Theory]
        public void RemoveTrailingSlash(string virtualPath, string expected)
        {
            Assert.Equal(expected, VirtualPathUtilityImpl.RemoveTrailingSlash(virtualPath));
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
            Assert.Equal(expected, VirtualPathUtilityImpl.GetDirectory(virtualPath));
        }

        [Fact]
        public void GetDirectoryError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetDirectory(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetDirectory(""));
            Assert.Throws<ArgumentException>(() => VirtualPathUtilityImpl.GetDirectory("test"));
            Assert.Throws<ArgumentException>(() => VirtualPathUtilityImpl.GetDirectory("test/world.jpg"));
        }

        [InlineData("/", "")]
        [InlineData("/test", "")]
        [InlineData("test", "")]
        [InlineData("/test/world.jpg", ".jpg")]
        [InlineData("test/world.jpg", ".jpg")]
        [Theory]
        public void GetExtension(string virtualPath, string expected)
        {
            Assert.Equal(expected, VirtualPathUtilityImpl.GetExtension(virtualPath));
        }

        [Fact]
        public void GetExtensionError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetExtension(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetExtension(""));
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
            Assert.Equal(expected, VirtualPathUtilityImpl.GetFileName(virtualPath));
        }

        [Fact]
        public void GetFileNameError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetFileName(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.GetFileName(""));
            Assert.Throws<ArgumentException>(() => VirtualPathUtilityImpl.GetFileName("test"));
            Assert.Throws<ArgumentException>(() => VirtualPathUtilityImpl.GetFileName("test/world.jpg"));
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
            Assert.Equal(expected, VirtualPathUtilityImpl.IsAbsolute(virtualPath));
        }

        [Fact]
        public void IsAbsoluteError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.IsAbsolute(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.IsAbsolute(""));
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
            Assert.Equal(expected, VirtualPathUtilityImpl.IsAppRelative(virtualPath));
        }

        [Fact]
        public void IsAppRelativeError()
        {
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.IsAppRelative(null!));
            Assert.Throws<ArgumentNullException>(() => VirtualPathUtilityImpl.IsAppRelative(""));
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
            var virtualPathUtility = CreateUtility();

            Assert.Equal(expected, virtualPathUtility.MakeRelative(fromPath, toPath));
        }

        [Fact]
        public void MakeRelativeError()
        {
            var virtualPathUtility = CreateUtility();

            Assert.Throws<NullReferenceException>(() => virtualPathUtility.MakeRelative("~/", null!));
            Assert.Throws<NullReferenceException>(() => virtualPathUtility.MakeRelative(null!, "~/"));
            Assert.Throws<ArgumentOutOfRangeException>(() => virtualPathUtility.MakeRelative("~/hello/", "test"));
            Assert.Throws<ArgumentOutOfRangeException>(() => virtualPathUtility.MakeRelative("test", "~/hello/"));
        }

        private static VirtualPathUtilityImpl CreateUtility()
        {
            var runtime = new Mock<IHttpRuntime>();

            runtime.Setup(r => r.AppDomainAppPath).Returns(@"C:\");
            runtime.Setup(r => r.AppDomainAppVirtualPath).Returns("/");

            return new VirtualPathUtilityImpl(runtime.Object);
        }
    }
}
