﻿using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePayrollInvoiceHandler : RequestHandler<CreatePayrollInvoiceCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPayrollService _payrollService;

    public CreatePayrollInvoiceHandler(
        ITenantUnityOfWork tenantUow,
        IPayrollService payrollService)
    {
        _tenantUow = tenantUow;
        _payrollService = payrollService;
    }

    protected override async Task<Result> HandleValidated(
        CreatePayrollInvoiceCommand req, CancellationToken cancellationToken)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result.Fail($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payroll = _payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
        await _tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
