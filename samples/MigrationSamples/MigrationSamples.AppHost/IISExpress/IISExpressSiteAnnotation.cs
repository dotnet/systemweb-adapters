using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.IISExpress;

internal class IISExpressSiteAnnotation : IResourceAnnotation
{
    public IISExpressSiteAnnotation(string applicationHostConfigPath, string appName)
    {
        this.ApplicationHostConfigPath = applicationHostConfigPath;
        this.SiteName = appName;
    }

    public string ApplicationHostConfigPath { get; }
    public string SiteName { get; }
}
