namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

/// <summary>
/// Platform-admin-only tenant fields. A null field is left unchanged.
/// </summary>
public class UpdateTenantAdminSettingsCommand : ICommand
{
    public Guid TenantId { get; set; }
    public string? Name { get; set; }
    public bool? AIEnabled { get; set; }
}
