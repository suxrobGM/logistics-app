using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>
/// Creates payroll invoices for multiple employees at once.
/// </summary>
[RequiresFeature(TenantFeature.Payroll)]
public record BatchCreatePayrollInvoicesCommand : ICommand<Result<BatchCreatePayrollInvoicesResult>>
{
    public required List<Guid> EmployeeIds { get; set; }
    public required DateOnly PeriodStart { get; set; }
    public required DateOnly PeriodEnd { get; set; }
}
