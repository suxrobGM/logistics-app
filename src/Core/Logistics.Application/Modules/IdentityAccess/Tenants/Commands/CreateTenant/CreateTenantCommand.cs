using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

public class CreateTenantCommand : ICommand
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public required Address CompanyAddress { get; set; }

    /// <summary>
    /// Operating mode the tenant starts in. Null falls back to <see cref="Domain.Primitives.Enums.OperatingMode.Fleet"/>.
    /// Only this one setting is accepted at creation - the rest of <c>TenantSettings</c> keeps its defaults.
    /// </summary>
    public OperatingMode? OperatingMode { get; set; }

    // Owner account created alongside the tenant
    public string OwnerEmail { get; set; } = null!;
    public string OwnerFirstName { get; set; } = null!;
    public string OwnerLastName { get; set; } = null!;
}
