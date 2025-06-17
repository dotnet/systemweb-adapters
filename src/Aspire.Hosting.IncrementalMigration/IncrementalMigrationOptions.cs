namespace Aspire.Hosting;

public class IncrementalMigrationOptions
{
    public RemoteAuthentication RemoteAuthentication { get; set; } = RemoteAuthentication.Disabled;

    public RemoteSession RemoteSession { get; set; } = RemoteSession.Disabled;

    public string RemoteAppEndpointName { get; set; } = "https";
}
