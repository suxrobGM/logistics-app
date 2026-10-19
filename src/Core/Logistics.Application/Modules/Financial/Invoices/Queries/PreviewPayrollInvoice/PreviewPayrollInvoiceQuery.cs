using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

[RequiresFeature(TenantFeature.Payroll)]
public class PreviewPayrollInvoiceQuery : IQuery<Result<InvoiceDto>>
{
    public Guid EmployeeId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
