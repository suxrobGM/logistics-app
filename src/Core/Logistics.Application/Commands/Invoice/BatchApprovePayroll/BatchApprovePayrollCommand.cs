using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class BatchApprovePayrollCommand : IAppRequest
{
    public required List<Guid> Ids { get; set; }
    public string? Notes { get; set; }
}
