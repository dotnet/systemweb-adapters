using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class RemoteSessionTests(ITestOutputHelper output, AspireFixture<SessionRemoteAppHost> aspire) : DebugPageTest, IClassFixture<AspireFixture<SessionRemoteAppHost>>
{
    [Fact]
    public async Task SessionTests()
    {
        await ValidateCount("/", 1, 0);
        await ValidateCount("/", 2, 0);
        await ValidateCount("/framework", 2, 1);
        await ValidateCount("/framework", 2, 2);

        // TODO: framework count currently gets reset on core
        await ValidateCount("/", 3, 0);
    }

    private async Task ValidateCount(string relative, int coreCount, int frameworkCount)
    {
        var app = await aspire.GetApplicationAsync();
        var endpoint = app.GetEndpoint("core");

        var response = await Page.GotoAsync(new Uri(endpoint, relative).ToString());

        Assert.NotNull(response);

        Assert.Equal(200, response.Status);

        var body = await response.BodyAsync();

        var str = Encoding.UTF8.GetString(body);
        output.WriteLine($"Response from '{relative}' ({coreCount}, {frameworkCount}): {str}");

        var data = JsonSerializer.Deserialize<SessionData[]>(body, JsonSerializerOptions.Web)
            !.ToDictionary(t => t.Key, t => t.Value);

        VerifyValue(data, "CoreCount", coreCount);
        VerifyValue(data, "FrameworkCount", frameworkCount);
    }

    private static void VerifyValue(Dictionary<string, int> data, string key, int expectedValue)
    {
        if (data.TryGetValue(key, out var value))
        {
            Assert.Equal(expectedValue, value);
        }
        else
        {
            Assert.Equal(0, expectedValue);
        }
    }

    private sealed class SessionData
    {
        public required string Key { get; init; }

        public int Value { get; init; }
    }
}
