using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

/// <summary>
/// Replaces a tenant's presets and resets its feature configurations to match. Admin-locked features keep their value.
/// </summary>
public class UpdateTenantPresetsCommand : ICommand
{
    public Guid TenantId { get; set; }
    public List<TenantPreset> Presets { get; set; } = [];
}
