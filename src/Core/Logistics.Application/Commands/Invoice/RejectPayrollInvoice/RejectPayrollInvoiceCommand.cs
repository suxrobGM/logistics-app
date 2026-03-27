using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class RejectPayrollInvoiceCommand : IAppRequest
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
}
