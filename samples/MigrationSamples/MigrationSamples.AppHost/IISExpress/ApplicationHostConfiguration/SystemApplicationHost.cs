using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

public class SystemApplicationHost
{
    [XmlArray("sites")]
    [XmlArrayItem("site", typeof(Site))]
    public required Site[] Sites { get; set; }
}
