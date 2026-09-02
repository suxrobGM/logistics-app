namespace Logistics.Shared.Models;

/// <summary>
/// Public discovery document served at /.well-known/logisticsx.json. Drives the noncommercial
/// banner in the portals and lets the author find deployed instances.
/// </summary>
public record ProductLicenseDiscoveryDto
{
    public string Product { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool Licensed { get; set; }
    public string? Licensee { get; set; }
}
