using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class PreviewPayrollInvoicesHandler(
    ITenantUnitOfWork tenantUow,
    IPayrollService payrollService)
    : IRequestHandler<PreviewPayrollInvoicesQuery, Result<InvoiceDto[]>>
{
    public async Task<Result<InvoiceDto[]>> Handle(PreviewPayrollInvoicesQuery req, CancellationToken ct)
    {
        var employees = await tenantUow.Repository<Employee>()
            .GetListAsync(e => req.EmployeeIds.Contains(e.Id), ct);

        var previews = new List<InvoiceDto>(employees.Count);
        foreach (var employee in employees)
        {
            var payrollInvoice = await payrollService.BuildPayrollInvoiceAsync(employee, req.PeriodStart, req.PeriodEnd, ct);
            previews.Add(payrollInvoice.ToDto());
        }

        return Result<InvoiceDto[]>.Ok(previews.ToArray());
    }
}
