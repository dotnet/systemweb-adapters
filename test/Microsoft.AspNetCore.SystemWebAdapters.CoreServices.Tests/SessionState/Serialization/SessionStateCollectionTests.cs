// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization.Tests;

public class SessionStateCollectionTests
{
    [Fact]
    public void EmptyState()
    {
        // Arrange
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act/Assert
        Assert.Equal(0, state.Count);
    }

    [Fact]
    public void EnableTracking()
    {
        // Arrange
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        state.SessionID = Guid.NewGuid().ToString();
        state.IsNewSession = true;
        state.IsAbandoned = true;
        state.Timeout = 5;

        const string SessionKey = "item1";
        var item1 = new object();
        state[SessionKey] = item1;

        // Act
        var tracking = state.WithTracking();

        // Assert
        Assert.IsAssignableFrom<ISessionStateChangeset>(tracking);
        Assert.Equal(1, state.Count);
        Assert.Equal(state.SessionID, tracking.SessionID);
        Assert.Equal(state.IsNewSession, tracking.IsNewSession);
        Assert.Equal(state.IsAbandoned, tracking.IsAbandoned);
        Assert.Equal(state.Timeout, tracking.Timeout);
        Assert.Same(state[SessionKey], tracking[SessionKey]);
    }

    [Fact]
    public void AddNewValue()
    {
        // Arrange
        const string Key = "key";
        object value = new();
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state[Key] = value;

        // Assert
        Assert.Same(state[Key], value);
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.New, c.State);
            });
    }

    [Fact]
    public void SetItem()
    {
        // Arrange
        const string Key = "key";
        byte[] value = [];
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.SetData(Key, value);

        // Assert
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.NoChange, c.State);
            });
    }

    [Fact]
    public void SetItemAndAccess()
    {
        // Arrange
        const string Key = "key";
        byte[] data = [];
        object? value = new();
        var serializer = new Mock<ISessionKeySerializer>();
        serializer.Setup(s => s.TryDeserialize(Key, data, out value)).Returns(true);
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.SetData(Key, data);
        var result = state[Key];

        // Assert
        Assert.Same(value, result);
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.Changed, c.State);
            });
    }

    [Fact]
    public void SetItemAndAccessButCannotDeserialize()
    {
        // Arrange
        const string Key = "key";
        byte[] data = [];
        object? value = new();
        var serializer = new Mock<ISessionKeySerializer>();
        serializer.Setup(s => s.TryDeserialize(Key, data, out value)).Returns(false);
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.SetData(Key, data);
        var result = state[Key];

        // Assert
        Assert.Null(result);
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.NoChange, c.State);
            });
    }

    [Fact]
    public void AddItemAndRemove()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);
        state[Key] = new();

        // Act
        state.Remove(Key);

        // Assert
        Assert.Empty(state.Changes);
    }

    [Fact]
    public void SetItemAndRemove()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);
        state.SetData(Key, []);

        // Act
        state.Remove(Key);

        // Assert
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.Removed, c.State);
            });
    }

    [Fact]
    public void MarkItemRemoved()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.MarkRemoved(Key);

        // Assert
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.Removed, c.State);
            });
    }

    [Fact]
    public void MarkItemUnchanged()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.MarkUnchanged(Key);

        // Assert
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.NoChange, c.State);
            });
    }

    [Fact]
    public void ClearAddedItems()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);
        state[Key] = new();

        // Act
        state.Clear();

        // Assert
        Assert.Empty(state.Changes);
    }

    [Fact]
    public void ClearSetItem()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);
        state.SetData(Key, []);

        // Act
        state.Clear();

        // Assert
        Assert.Collection(state.Changes,
            c =>
            {
                Assert.Equal(Key, c.Key);
                Assert.Equal(SessionItemChangeState.Removed, c.State);
            });
    }

    [Fact]
    public void SetUnknownKey()
    {
        // Arrange
        const string Key = "key";
        var serializer = new Mock<ISessionKeySerializer>();
        using var state = new SessionStateCollection(serializer.Object);

        // Act
        state.SetData(Key, []);
        state.SetUnknownKey(Key);

        // Assert
        Assert.Equal(0, state.Count);
        Assert.Empty(state.Keys);
        Assert.Empty(state.Changes);
        Assert.Equal([Key], state.UnknownKeys);
    }
}

