using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class PreviewPayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IPayrollService payrollService)
    : IAppRequestHandler<PreviewPayrollInvoiceQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(
        PreviewPayrollInvoiceQuery req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result<InvoiceDto>.Fail($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payrollInvoice = payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
        var payrollInvoiceDto = payrollInvoice.ToDto();
        return Result<InvoiceDto>.Ok(payrollInvoiceDto);
    }
}
