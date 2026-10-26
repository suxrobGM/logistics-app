using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

internal sealed class AccountingPayablesSyncService(
    ITenantUnitOfWork tenantUow,
    IAccountingMappingService mappingService,
    ILogger<AccountingPayablesSyncService> logger) : IAccountingPayablesSyncService
{
    public async Task PushAsync(SyncRun run, CancellationToken ct)
    {
        await PushFactoringFeesAsync(run, ct);
        await PushExpensesAsync(run, ct);
        await PushPayrollAsync(run, ct);
    }

    /// <summary>
    /// The factor's fee is a purchase paid from the clearing account; the bookkeeper records the advance
    /// and reserve transfers. Fees on invoices before the sync-from date are skipped, as their deposit is never pushed.
    /// </summary>
    private async Task PushFactoringFeesAsync(SyncRun run, CancellationToken ct)
    {
        var clearingAccountId = run.Config.FactoringClearingAccountId;
        var feeAccountId = run.Config.FactoringFeeAccountId;
        if (clearingAccountId is null || feeAccountId is null)
        {
            return;
        }

        var from = run.SyncFrom;
        var fees = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(mappingService.NotUpToDate<FactoringBatchItem>(QboLocalEntityType.FactoringFee))
            .Where(i => (i.Status == FactoringStatus.Funded || i.Status == FactoringStatus.ChargedBack)
                        && i.FeeAmount > 0 && i.FundedAt >= from && i.Invoice.CreatedAt >= from)
            .OrderBy(i => i.FundedAt)
            .Take(SyncRun.BatchSize)
            .Select(i => new { Item = i, BatchNumber = i.Batch.Number, InvoiceNumber = i.Invoice.Number })
            .ToListAsync(ct);

        await mappingService.LoadAsync(run, QboLocalEntityType.FactoringFee, fees.Select(f => f.Item.Id), ct);

        foreach (var fee in fees)
        {
            var (payload, hash) = AccountingPayloads.FactoringFee(
                fee.Item, fee.BatchNumber, fee.InvoiceNumber, clearingAccountId, feeAccountId);
            await mappingService.PushAsync(run, QboLocalEntityType.FactoringFee, fee.Item.Id, hash,
                target => run.Provider.UpsertExpenseAsync(payload, target, ct), ct);
        }
    }

    private async Task PushExpensesAsync(SyncRun run, CancellationToken ct)
    {
        var config = run.Config;
        if (config.DefaultPaymentAccountId is null || config.DefaultExpenseAccountId is null)
        {
            logger.LogInformation("Skipping expense sync: the QuickBooks payment and expense accounts are not set");
            return;
        }

        var from = run.SyncFrom;
        var expenses = await tenantUow.Repository<Expense>().Query()
            .Where(mappingService.NotUpToDate<Expense>(QboLocalEntityType.Expense))
            .Where(e => (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid) && e.ExpenseDate >= from)
            .OrderBy(e => e.CreatedAt)
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        await mappingService.LoadAsync(run, QboLocalEntityType.Expense, expenses.Select(e => e.Id), ct);

        foreach (var expense in expenses)
        {
            var accountId = AccountingPayloads.ExpenseAccountId(expense, config, config.Mappings)!;
            var (payload, hash) = AccountingPayloads.Expense(
                expense, config.DefaultPaymentAccountId, config.DefaultPaymentAccountType, accountId);
            await mappingService.PushAsync(run, QboLocalEntityType.Expense, expense.Id, hash,
                target => run.Provider.UpsertExpenseAsync(payload, target, ct), ct);
        }
    }

    /// <summary>Each driver is pushed as a vendor first, so their pay bills have someone to be owed to.</summary>
    private async Task PushPayrollAsync(SyncRun run, CancellationToken ct)
    {
        var accountId = run.Config.PayrollExpenseAccountId;
        if (accountId is null)
        {
            return;
        }

        var from = DateOnly.FromDateTime(run.SyncFrom);
        var payrolls = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(mappingService.NotUpToDate<PayrollInvoice>(QboLocalEntityType.PayrollInvoice))
            .Where(p => (p.Status == InvoiceStatus.Approved || p.Status == InvoiceStatus.PartiallyPaid
                         || p.Status == InvoiceStatus.Paid) && p.PeriodEnd >= from)
            .OrderBy(p => p.CreatedAt)
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        if (payrolls.Count == 0)
        {
            return;
        }

        var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
        var employees = (await tenantUow.Repository<Employee>().GetListAsync(e => employeeIds.Contains(e.Id), ct))
            .ToDictionary(e => e.Id);
        await mappingService.LoadAsync(run, QboLocalEntityType.PayrollInvoice, payrolls.Select(p => p.Id), ct);
        await mappingService.LoadAsync(run, QboLocalEntityType.Employee, employeeIds, ct);

        foreach (var employee in employees.Values)
        {
            var (vendor, vendorHash) = AccountingPayloads.Vendor(employee);
            await mappingService.PushAsync(run, QboLocalEntityType.Employee, employee.Id, vendorHash,
                target => run.Provider.UpsertVendorAsync(vendor, target, ct), ct);
        }

        foreach (var payroll in payrolls)
        {
            if (!employees.TryGetValue(payroll.EmployeeId, out var employee))
            {
                continue;
            }

            var vendorQboId = run.SyncedQboId(QboLocalEntityType.Employee, employee.Id);
            if (vendorQboId is null)
            {
                await mappingService.RecordFailureAsync(run, QboLocalEntityType.PayrollInvoice, payroll.Id,
                    $"{employee.FirstName} {employee.LastName} could not be added as a QuickBooks vendor. Fix the employee's sync error first.", ct);
                continue;
            }

            var (bill, hash) = AccountingPayloads.Bill(payroll, vendorQboId, accountId);
            await mappingService.PushAsync(run, QboLocalEntityType.PayrollInvoice, payroll.Id, hash,
                target => run.Provider.UpsertBillAsync(bill, target, ct), ct);
        }
    }
}
