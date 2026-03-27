using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class MapEldDriverCommand : IAppRequest
{
    public Guid EmployeeId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
}
