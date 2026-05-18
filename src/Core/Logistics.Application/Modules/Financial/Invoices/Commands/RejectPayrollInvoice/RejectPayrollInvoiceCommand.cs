using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class RejectPayrollInvoiceCommand : ICommand
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
}
