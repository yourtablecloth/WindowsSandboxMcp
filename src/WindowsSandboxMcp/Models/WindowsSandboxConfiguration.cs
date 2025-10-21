using System.Data;
using System.Xml.Linq;

public sealed class WindowsSandboxConfiguration
{
    public bool? EnableVGpu { get; set; }
    public bool? EnableNetworking { get; set; }
    public List<MappedFolderConfiguration> MappedFolders { get; } = [];
    public string? LogonCommand { get; set; }
    public bool? EnableAudioInput { get; set; }
    public bool? EnableVideoInput { get; set; }
    public bool? EnableProtectedClient { get; set; }
    public bool? EnablePrinterRedirection { get; set; }
    public bool? EnableClipboardRedirection { get; set; }
    public int? MemoryInMB { get; set; }

    public XElement ToXmlElement()
    {
        var elem = new XElement("Configuration");

        if (EnableVGpu.HasValue)
            elem.Add(new XElement("vGPU", EnableVGpu.Value ? "Enable" : "Disable"));

        if (EnableNetworking.HasValue)
            elem.Add(new XElement("Networking", EnableNetworking.Value ? "Enable" : "Disable"));

        if (MappedFolders.Any())
            elem.Add("MappedFolders", MappedFolders.Select(x => x.ToXmlElement()));

        if (LogonCommand != null)
            elem.Add(new XElement("LogonCommand", new XElement("Command", LogonCommand)));

        if (EnableAudioInput.HasValue)
            elem.Add(new XElement("AudioInput", EnableAudioInput.Value ? "Enable" : "Disable"));

        if (EnableVideoInput.HasValue)
            elem.Add(new XElement("VideoInput", EnableVideoInput.Value ? "Enable" : "Disable"));

        if (EnableProtectedClient.HasValue)
            elem.Add(new XElement("ProtectedClient", EnableProtectedClient.Value ? "Enable" : "Disable"));

        if (EnablePrinterRedirection.HasValue)
            elem.Add(new XElement("PrinterRedirection", EnablePrinterRedirection.Value ? "Enable" : "Disable"));

        if (EnableClipboardRedirection.HasValue)
            elem.Add(new XElement("ClipboardRedirection", EnableClipboardRedirection.Value ? "Enable" : "Disable"));

        if (MemoryInMB.HasValue)
            elem.Add(new XElement("MemoryInMB", MemoryInMB.Value.ToString()));

        return elem;
    }

    public override string ToString()
    {
        var elem = ToXmlElement();
        if (elem.HasElements)
            return elem.ToString(SaveOptions.DisableFormatting);
        return string.Empty;
    }
}
