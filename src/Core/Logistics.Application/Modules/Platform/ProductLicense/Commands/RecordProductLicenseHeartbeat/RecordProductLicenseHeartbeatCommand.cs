namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

/// <summary>
/// Inbound daily heartbeat from a deployed instance. Anonymous and rate limited; the body is
/// the same shape as <see cref="Logistics.Shared.Models.ProductLicenseHeartbeatDto"/>.
/// </summary>
public sealed class RecordProductLicenseHeartbeatCommand : ICommand
{
    public Guid InstanceId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? KeyId { get; set; }
    public string? Licensee { get; set; }
    public int TenantCount { get; set; }
}
