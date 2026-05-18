using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class MapEldDriverCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
}
