using System.Xml.Serialization;

namespace C3D.Extensions.Aspire.IISExpress.Configuration;

public class Binding
{
    [XmlAttribute("protocol")]
    public required string Protocol { get; set; }

    [XmlAttribute("bindingInformation")]
    public required string BindingInformation { get; set; }

    public int Port => int.Parse(BindingInformation.Split(':')[1]);
}