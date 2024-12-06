using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.IISExpress;

public class IISExpressProjectResource(string name, string path, string workingDirectory)
    : ExecutableResource(name, path, workingDirectory), IResourceWithServiceDiscovery
{
}
