using System.Security.Cryptography;
using Aspire.Hosting.ApplicationModel;
using Projects;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

public class MachineKeyTests(AspireFixture<MachineKeyAppHost> aspire) : IClassFixture<AspireFixture<MachineKeyAppHost>>
{
    [Theory]
    [InlineData(AppEndpoint.Core, AppEndpoint.Framework)]
    [InlineData(AppEndpoint.Framework, AppEndpoint.Core)]

    public async Task RoundtripBetweenServices(AppEndpoint source, AppEndpoint destination)
    {
        var app = await aspire.GetApplicationAsync();

        using var sourceClient = app.CreateHttpClient(GetName(source));
        using var destinationClient = app.CreateHttpClient(GetName(destination));

        var data = RandomNumberGenerator.GetBytes(1024);

        var @protected = await RunAsync(sourceClient, InputAction.Protect, data);
        var unprotected = await RunAsync(destinationClient, InputAction.Unprotect, @protected);

        Assert.Equal(data, unprotected);

        static string GetName(AppEndpoint name) => name switch
        {
            AppEndpoint.Core => "core",
            AppEndpoint.Framework => "framework",
            _ => throw new InvalidOperationException(),
        };

        static async Task<byte[]> RunAsync(HttpClient client, InputAction action, byte[] data)
        {
            var machineKeyPath = new Uri("/?output=simple", UriKind.Relative);
            using var content = new FormUrlEncodedContent([
                new("action", action.ToString()),
                new("purposes", ""),
                new("data", Convert.ToBase64String(data))
            ]);

            using var result = await client.PostAsync(machineKeyPath, content);

            var str = await result.Content.ReadAsStringAsync();

            if (result.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Error calling service: {str}");
            }

            return Convert.FromBase64String(str);
        }
    }

    public enum AppEndpoint
    {
        Core,
        Framework,
    }

    private enum InputAction
    {
        Protect,
        Unprotect,
    }
}
