﻿using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class PreviewPayrollInvoiceHandler : RequestHandler<PreviewPayrollInvoiceQuery, Result<InvoiceDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPayrollService _payrollService;

    public PreviewPayrollInvoiceHandler(
        ITenantUnityOfWork tenantUow,
        IPayrollService payrollService)
    {
        _tenantUow = tenantUow;
        _payrollService = payrollService;
    }

    protected override async Task<Result<InvoiceDto>> HandleValidated(
        PreviewPayrollInvoiceQuery req, CancellationToken cancellationToken)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result<InvoiceDto>.Fail($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payrollInvoice = _payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
        var payrollInvoiceDto = payrollInvoice.ToDto();
        return Result<InvoiceDto>.Succeed(payrollInvoiceDto);
    }
}
