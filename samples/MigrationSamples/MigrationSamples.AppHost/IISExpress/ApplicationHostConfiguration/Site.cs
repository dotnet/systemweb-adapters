using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

public class Site
{
    [XmlAttribute("name")]
    public required string Name { get; set; }

    [XmlElement("application")]
    public required Application Application { get; set; }

    [XmlArray("bindings")]
    [XmlArrayItem("binding", typeof(Binding))]
    public required Binding[] Bindings { get; set; }
}
