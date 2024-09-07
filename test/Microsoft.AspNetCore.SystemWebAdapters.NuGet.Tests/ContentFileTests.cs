// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.NuGet.Tests;

public class ContentFileTests
{
    [Fact]
    public void ContentFilesCopied()
    {
        // Arrange
        var expected = new[]
        {
            Path.Combine("Scripts", "jquery-3.5.1-vsdoc.js"),
            Path.Combine("Scripts", "jquery-3.5.1.js"),
            Path.Combine("Scripts", "jquery-3.5.1.min.js"),
            Path.Combine("Scripts", "jquery-3.5.1.min.map"),
            Path.Combine("Scripts", "jquery-3.5.1.slim.js"),
            Path.Combine("Scripts", "jquery-3.5.1.slim.min.js"),
            Path.Combine("Scripts", "jquery-3.5.1.slim.min.map"),
        };

        // Act
        var files = Directory.GetFiles("Scripts");

        // Assert
        Assert.Equal(expected.Order(StringComparer.Ordinal), files.Order(StringComparer.Ordinal));
    }
}
