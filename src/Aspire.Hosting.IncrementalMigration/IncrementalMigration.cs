using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public class IncrementalMigration(string name) : Resource(name)
{
    internal RemoteAuthentication RemoteAuthentication { get; set; } = RemoteAuthentication.Disabled;

    internal RemoteSession RemoteSession { get; set; } = RemoteSession.Disabled;

    internal string RemoteAppEndpointName { get; set; } = "https";
}
