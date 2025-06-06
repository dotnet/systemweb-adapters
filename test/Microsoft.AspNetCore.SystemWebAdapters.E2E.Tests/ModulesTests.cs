using Projects;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class ModulesTests(ITestOutputHelper output, AspireFixture<ModulesAppHost> aspire) : IClassFixture<AspireFixture<ModulesAppHost>>
{
    [Fact]
    public async Task ValidateModuleFiringOrder()
    {
        var coreModules = await GetModules("core");
        var frameworkModules = await GetModules("framework");

        Assert.Equal(coreModules, frameworkModules);
    }

    private async Task<List<string>> GetModules(string name)
    {
        var baseUrl = new Uri("/", UriKind.Relative);

        var app = await aspire.GetApplicationAsync();
        using var client = app.CreateHttpClient(name);
        using var response = await client.GetAsync(baseUrl);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        output.WriteLine($"Reading output for {name}...");
        output.WriteLine("==================");

        var list = new List<string>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            Assert.NotNull(line);

            if (line == "PreSendRequestHeaders" || line == "PreSendRequestContent")
            {
                // TODO: These are currently in a different order
                output.WriteLine($"::{line}");
                continue;
            }

            output.WriteLine(line);

            list.Add(line);
        }

        output.WriteLine("==================");

        return list;
    }
}
