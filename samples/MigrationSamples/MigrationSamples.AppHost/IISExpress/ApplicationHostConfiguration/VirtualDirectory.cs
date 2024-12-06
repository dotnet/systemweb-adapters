using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

public class VirtualDirectory
{
    [XmlAttribute("physicalPath")]
    public required string PhysicalPath { get; set; }
}
