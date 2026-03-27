using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class SubmitPayrollForApprovalCommand : IAppRequest
{
    public Guid Id { get; set; }
}
