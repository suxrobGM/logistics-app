using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

/// <summary>
/// Inbound daily heartbeat from a deployed instance. Anonymous and rate limited. Carries the
/// sender's own DTO so the two ends of the wire cannot drift apart.
/// </summary>
public sealed record RecordProductLicenseHeartbeatCommand(ProductLicenseHeartbeatDto Report) : ICommand;
