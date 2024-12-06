using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.IISExpress.Resources;

public class IISExpressConfigurationResource : Resource, IManifestExpressionProvider
{
    public IISExpressConfigurationResource(string solutionName, string solutionDir) : base("Solution")
    {
        SolutionName = solutionName;
        SolutionDir = solutionDir;
    }

    public string SolutionName { get; private set; }
    public string SolutionDir { get; private set; }

    public string ValueExpression => throw new NotImplementedException();
}