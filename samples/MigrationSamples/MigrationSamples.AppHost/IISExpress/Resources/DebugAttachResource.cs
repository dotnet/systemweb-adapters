using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.IISExpress.Resources;

internal class DebugAttachResource : IResourceAnnotation
{
    public DebugMode DebugMode { get; set; }
}