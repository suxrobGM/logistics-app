using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

/// <summary>
/// Creates payroll invoices for multiple employees at once.
/// </summary>
[RequiresFeature(TenantFeature.Payroll)]
public record BatchCreatePayrollInvoicesCommand : ICommand<Result<BatchCreatePayrollInvoicesResult>>
{
    public required List<Guid> EmployeeIds { get; set; }
    public required DateTime PeriodStart { get; set; }
    public required DateTime PeriodEnd { get; set; }
}
