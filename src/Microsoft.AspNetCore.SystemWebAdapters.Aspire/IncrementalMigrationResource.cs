using Aspire.Hosting.ApplicationModel;

namespace Microsoft.AspNetCore.SystemWebAdapters.Aspire;

public class IncrementalMigrationResource : Resource
{
    private readonly ParameterResource apiKey;

    internal IncrementalMigrationResource(string name, ParameterResource apiKey) : base(name)
    {
        this.apiKey = apiKey;
    }

    public ParameterResource ApiKey => apiKey;

    internal bool IsProxySupported { get; set; }

    internal bool IsRemoteEnabled => IsRemoteAuthenticationSupported || IsRemoteSessionSupported;

    internal bool IsRemoteAuthenticationSupported { get; set; }

    internal bool IsRemoteSessionSupported { get; set; }
}
