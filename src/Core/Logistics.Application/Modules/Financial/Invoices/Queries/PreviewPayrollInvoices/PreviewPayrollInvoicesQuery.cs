using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

/// <summary>
/// Previews the payroll invoice of each employee for one period, without saving anything.
/// Unknown employee IDs are skipped.
/// </summary>
[RequiresFeature(TenantFeature.Payroll)]
public class PreviewPayrollInvoicesQuery : IQuery<Result<InvoiceDto[]>>
{
    public required List<Guid> EmployeeIds { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
