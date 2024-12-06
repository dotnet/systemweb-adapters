using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

[XmlRoot("configuration")]
public class ApplicationHostConfiguration
{
    [XmlElement("system.applicationHost")]
    public required SystemApplicationHost SystemApplicationHost { get; set; }
}
