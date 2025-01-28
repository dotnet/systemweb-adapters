// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Wrapped.Tests;

public class CompositeSessionKeySerializerTests
{
    [Fact]
    public void MultipleDeserializers()
    {
        // Arrange
        var bytes1 = new byte[] { 1 };
        var bytes2 = new byte[] { 2 };
        var obj1 = new object();
        var obj2 = new object();

        var serializer1 = new Mock<ISessionKeySerializer>();
        serializer1.Setup(s => s.TryDeserialize("key1", bytes1, out obj1)).Returns(true);
        var serializer2 = new Mock<ISessionKeySerializer>();
        serializer2.Setup(s => s.TryDeserialize("key2", bytes2, out obj2)).Returns(true);
        var logger = new Mock<ILogger<CompositeSessionKeySerializer>>();

        // Act
        var combined = new CompositeSessionKeySerializer(new[] { serializer1.Object, serializer2.Object }, Options.Create(new SessionSerializerOptions()), logger.Object);

        // Assert
        Assert.True(combined.TryDeserialize("key1", bytes1, out var result1));
        Assert.Same(obj1, result1);
        Assert.True(combined.TryDeserialize("key2", bytes2, out var result2));
        Assert.Same(obj2, result2);
    }

    [Fact]
    public void MultipleSerializers()
    {
        // Arrange
        var bytes1 = new byte[] { 1 };
        var bytes2 = new byte[] { 2 };
        var obj1 = new object();
        var obj2 = new object();

        var serializer1 = new Mock<ISessionKeySerializer>();
        serializer1.Setup(s => s.TrySerialize("key1", obj1, out bytes1)).Returns(true);
        var serializer2 = new Mock<ISessionKeySerializer>();
        serializer2.Setup(s => s.TrySerialize("key2", obj2, out bytes2)).Returns(true);
        var logger = new Mock<ILogger<CompositeSessionKeySerializer>>();

        // Act
        var combined = new CompositeSessionKeySerializer(new[] { serializer1.Object, serializer2.Object }, Options.Create(new SessionSerializerOptions()), logger.Object); ;

        // Assert
        Assert.True(combined.TrySerialize("key1", obj1, out var result1));
        Assert.Same(bytes1, result1);
        Assert.True(combined.TrySerialize("key2", obj2, out var result2));
        Assert.Same(bytes2, result2);
    }
}
