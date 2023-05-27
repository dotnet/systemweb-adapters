using System.IO;
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
            """Scripts\jquery-3.5.1-vsdoc.js""",
            """Scripts\jquery-3.5.1.js""",
            """Scripts\jquery-3.5.1.min.js""",
            """Scripts\jquery-3.5.1.min.map""",
            """Scripts\jquery-3.5.1.slim.js""",
            """Scripts\jquery-3.5.1.slim.min.js""",
            """Scripts\jquery-3.5.1.slim.min.map""",
        };

        // Act
        var files = Directory.GetFiles("Scripts");

        // Assert
        Assert.Equal(expected, files);
    }

    [Fact]
    public void ToolsFiles()
    {
        // Arrange
        const string Path = "test_tools_output.txt";
        var content = File.ReadAllLines(Path);

        // Assert
        Assert.Collection(
            content,
            c => Assert.Equal("jQuery", c));
    }
}
