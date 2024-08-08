// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class BuilderTests
{
    [Fact]
    public void MultipleServiceRegistrationInvocations()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSystemWebAdapters();

        var count = services.Count;

        // Act
        services.AddSystemWebAdapters();

        // Assert
        Assert.Equal(count, services.Count);
    }

    [Fact]
    public void MultipleServiceHttpApplicationRegistrationInvocations()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSystemWebAdapters()
            .AddHttpApplication();

        var count = services.Count;

        // Act
        services.AddSystemWebAdapters()
            .AddHttpApplication();

        // Assert
        Assert.Equal(count, services.Count);
    }
}
