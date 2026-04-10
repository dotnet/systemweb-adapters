using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

internal static partial class DockerStatus
{
    public static bool IsSupported => _linuxContainerStatus.Value.IsSupported;

    public static string? OS => _linuxContainerStatus.Value.OS;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    private static Lazy<(bool IsSupported, string? OS)> _linuxContainerStatus = new(() =>
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version --format json",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return (false, null);
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return (false, null);
            }

            var result = JsonSerializer.Deserialize(output, DockerJsonContext.Default.DockerVersionInfo);

            return (string.Equals(result?.Server?.Os, "linux", StringComparison.OrdinalIgnoreCase), result?.Server?.Os);
        }
        catch
        {
            return (false, null);
        }
    });

    [JsonSerializable(typeof(DockerVersionInfo))]
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    private partial class DockerJsonContext : JsonSerializerContext
    {
    }

    public class DockerVersionInfo
    {
        public Server? Server { get; set; }
    }

    public class Server
    {
        public string? Os { get; set; }
    }
}
