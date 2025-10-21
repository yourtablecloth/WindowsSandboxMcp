using System.Xml.Linq;

public sealed class MappedFolderConfiguration
{
    public required string HostFolderPath { get; set; }
    public string? SandboxFolderAbsolutePath { get; set; }
    public bool? ReadOnly { get; set; }

    public XElement ToXmlElement()
    {
        var elem = new XElement("MappedFolder",
            new XElement("HostFolder", this.HostFolderPath));
        if (SandboxFolderAbsolutePath != null)
            elem.Add(new XElement("SandboxFolder", this.SandboxFolderAbsolutePath));
        if (ReadOnly.HasValue)
            elem.Add(new XElement("ReadOnly", ReadOnly.Value));
        return elem;
    }

    public override string ToString()
        => ToXmlElement().ToString(SaveOptions.DisableFormatting);
}
