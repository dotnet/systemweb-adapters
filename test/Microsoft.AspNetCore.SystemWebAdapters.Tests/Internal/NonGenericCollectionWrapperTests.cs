// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    [SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Testing collection implementation")]
    public class NonGenericCollectionWrapperTests
    {
        private readonly Fixture _fixture;

        public NonGenericCollectionWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Count()
        {
            var original = _fixture.CreateMany<int>().ToArray();
            var wrapped = new NonGenericCollectionWrapper<int>(original);

            Assert.Equal(original.Length, wrapped.Count);
        }

        [Fact]
        public void Empty()
        {
            var wrapped = new NonGenericCollectionWrapper<object>(Array.Empty<object>());

            Assert.Equal(0, wrapped.Count);
            Assert.Empty(wrapped);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(true)]
        [InlineData(false)]
        public void IsSynchronized(bool? isSynchronized)
        {
            // Arrange
            var original = new Mock<ICollection<object>>();

            if (isSynchronized.HasValue)
            {
                var collection = original.As<ICollection>();
                collection.Setup(c => c.IsSynchronized).Returns(isSynchronized.Value);
            }

            var wrapped = new NonGenericCollectionWrapper<object>(original.Object);

            // Act
            var result = wrapped.IsSynchronized;

            // Assert
            Assert.Equal(isSynchronized ?? false, result);
        }

        [Fact]
        public void NoSyncRoot()
        {
            // Arrange
            var original = new Mock<ICollection<object>>();
            var wrapped = new NonGenericCollectionWrapper<object>(original.Object);

            // Act
            var result1 = wrapped.SyncRoot;
            var result2 = wrapped.SyncRoot;

            // Assert
            Assert.Same(result1, result2);
        }

        [Fact]
        public void HasSyncRoot()
        {
            // Arrange
            var original = new Mock<ICollection<object>>();
            var syncRoot = new object();

            var collection = original.As<ICollection>();
            collection.Setup(c => c.SyncRoot).Returns(syncRoot);

            var wrapped = new NonGenericCollectionWrapper<object>(original.Object);

            // Act
            var result1 = wrapped.SyncRoot;
            var result2 = wrapped.SyncRoot;

            // Assert
            Assert.Same(syncRoot, result1);
            Assert.Same(syncRoot, result2);
        }

        [Fact]
        public void SyncProperties()
        {
            var list = new List<object>();
            var wrapped = new NonGenericCollectionWrapper<object>(list);

            Assert.False(wrapped.IsSynchronized);
            Assert.Equal(((ICollection)list).SyncRoot, wrapped.SyncRoot);
        }

        [Fact]
        public void CopyToNull()
        {
            var wrapped = new NonGenericCollectionWrapper<object>(Array.Empty<object>());

            Assert.Throws<ArgumentNullException>(() => wrapped.CopyTo(null!, 0));
        }

        [Fact]
        public void CopyTo()
        {
            var original = _fixture.CreateMany<int>().ToArray();
            var wrapped = new NonGenericCollectionWrapper<int>(original);
            var destination = new int[original.Length];

            wrapped.CopyTo(destination, 0);

            Assert.True(original.SequenceEqual(destination));
        }
    }
}
