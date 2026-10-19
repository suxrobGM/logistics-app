using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal partial class PayrollService
{
    public async Task ReleaseDeductionsAsync(PayrollInvoice payroll, CancellationToken ct = default)
    {
        var advances = await tenantUow.Repository<CashAdvance>()
            .GetListAsync(a => a.PayrollInvoiceId == payroll.Id, ct);
        foreach (var advance in advances)
        {
            advance.PayrollInvoiceId = null;
        }

        var fuel = await tenantUow.Repository<FuelCardTransaction>()
            .GetListAsync(t => t.PayrollInvoiceId == payroll.Id, ct);
        foreach (var transaction in fuel)
        {
            transaction.PayrollInvoiceId = null;
        }
    }

    public async Task<decimal> GetEscrowBalanceAsync(
        Guid employeeId, Guid? exceptPayrollId = null, CancellationToken ct = default)
    {
        var held = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(p => p.EmployeeId == employeeId &&
                        p.Id != exceptPayrollId &&
                        p.Status != InvoiceStatus.Rejected &&
                        p.Status != InvoiceStatus.Cancelled)
            .SelectMany(p => p.LineItems)
            .Where(li => li.Type == InvoiceLineItemType.Escrow)
            .SumAsync(li => li.Amount.Amount * li.Quantity, ct);

        // Contributions are stored as negative lines.
        return -held;
    }

    private Task<List<CashAdvance>> GetOwedCashAdvancesAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employeeId = payroll.Employee.Id;

        // Rows this payroll already claimed still carry its id in the database during a recalculation:
        // ReleaseDeductionsAsync clears it only in memory.
        return tenantUow.Repository<CashAdvance>().GetListAsync(a =>
            a.EmployeeId == employeeId &&
            (a.PayrollInvoiceId == null || a.PayrollInvoiceId == payroll.Id), ct);
    }

    /// <summary>
    /// Matched fuel card purchases in the period on trucks the employee is the main driver of. The
    /// co-driver is never charged, so a team truck's fuel is not deducted twice.
    /// </summary>
    private async Task<List<FuelCardTransaction>> GetFuelChargesAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employeeId = payroll.Employee.Id;
        var (from, to) = payroll.PeriodUtcRange();
        var trucks = tenantUow.Repository<Truck>().Query();

        return await tenantUow.Repository<FuelCardTransaction>().GetListAsync(t =>
            trucks.Any(truck => truck.Id == t.TruckId && truck.MainDriverId == employeeId) &&
            t.Status == FuelCardTransactionStatus.Matched &&
            (t.PayrollInvoiceId == null || t.PayrollInvoiceId == payroll.Id) &&
            t.TransactionDate >= from && t.TransactionDate < to, ct);
    }

    /// <summary>Every owed advance and fuel charge in the inputs becomes a settlement line, so all are claimed.</summary>
    private static void ClaimDeductions(PayrollInvoice payroll, SettlementInputs inputs)
    {
        foreach (var advance in inputs.CashAdvances)
        {
            advance.PayrollInvoiceId = payroll.Id;
        }

        foreach (var fuel in inputs.FuelCharges)
        {
            fuel.PayrollInvoiceId = payroll.Id;
        }
    }
}
