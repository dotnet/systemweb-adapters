using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

public class Application
{
    [XmlElement("virtualDirectory")]
    public required VirtualDirectory VirtualDirectory { get; set; }
}
