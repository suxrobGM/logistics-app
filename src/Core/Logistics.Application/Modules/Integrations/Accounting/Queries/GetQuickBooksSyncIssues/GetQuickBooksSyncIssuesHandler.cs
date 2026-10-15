using System.Globalization;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

internal sealed class GetQuickBooksSyncIssuesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetQuickBooksSyncIssuesQuery, Result<List<AccountingSyncIssueDto>>>
{
    private const int MaxIssues = 200;

    public async Task<Result<List<AccountingSyncIssueDto>>> Handle(GetQuickBooksSyncIssuesQuery req, CancellationToken ct)
    {
        var failed = await tenantUow.Repository<QboEntityMapping>().Query()
            .Where(m => m.SyncStatus == QboSyncStatus.Failed)
            .OrderByDescending(m => m.LastAttemptAt)
            .Take(MaxIssues)
            .ToListAsync(ct);

        var labels = await LoadLabelsAsync(failed, ct);

        return Result<List<AccountingSyncIssueDto>>.Ok(failed
            .Select(m => new AccountingSyncIssueDto
            {
                Id = m.Id,
                EntityType = m.LocalEntityType,
                LocalId = m.LocalId,
                Label = labels.GetValueOrDefault(m.LocalId) ?? $"Deleted {m.LocalEntityType.GetDescription().ToLowerInvariant()}",
                Error = m.LastError,
                LastAttemptAt = m.LastAttemptAt
            })
            .ToList());
    }

    /// <summary>One query per record type, so the table names each record without a lookup per row.</summary>
    private async Task<Dictionary<Guid, string>> LoadLabelsAsync(List<QboEntityMapping> failed, CancellationToken ct)
    {
        List<Guid> Ids(QboLocalEntityType type) =>
            failed.Where(m => m.LocalEntityType == type).Select(m => m.LocalId).ToList();

        var customerIds = Ids(QboLocalEntityType.Customer);
        var invoiceIds = Ids(QboLocalEntityType.Invoice);
        var paymentIds = Ids(QboLocalEntityType.Payment);
        var expenseIds = Ids(QboLocalEntityType.Expense);
        var payrollIds = Ids(QboLocalEntityType.PayrollInvoice);
        var employeeIds = Ids(QboLocalEntityType.Employee);
        var factoringFeeIds = Ids(QboLocalEntityType.FactoringFee);

        var labels = new Dictionary<Guid, string>();
        foreach (var c in await tenantUow.Repository<Customer>().GetListAsync(c => customerIds.Contains(c.Id), ct))
        {
            labels[c.Id] = c.Name;
        }

        foreach (var i in await tenantUow.Repository<LoadInvoice>().GetListAsync(i => invoiceIds.Contains(i.Id), ct))
        {
            labels[i.Id] = $"Invoice {Num(i.Number)}";
        }

        foreach (var p in await tenantUow.Repository<Payment>().GetListAsync(p => paymentIds.Contains(p.Id), ct))
        {
            labels[p.Id] = $"Payment {p.ReferenceNumber ?? $"{p.Amount.Amount.ToString(CultureInfo.InvariantCulture)} {p.Amount.Currency}"}";
        }

        foreach (var e in await tenantUow.Repository<Expense>().GetListAsync(e => expenseIds.Contains(e.Id), ct))
        {
            labels[e.Id] = $"Expense {Num(e.Number)}{(string.IsNullOrEmpty(e.VendorName) ? "" : $" ({e.VendorName})")}";
        }

        foreach (var p in await tenantUow.Repository<PayrollInvoice>().GetListAsync(p => payrollIds.Contains(p.Id), ct))
        {
            labels[p.Id] = $"Driver pay {Num(p.Number)}";
        }

        foreach (var e in await tenantUow.Repository<Employee>().GetListAsync(e => employeeIds.Contains(e.Id), ct))
        {
            labels[e.Id] = $"{e.FirstName} {e.LastName} (vendor)";
        }

        var fees = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(i => factoringFeeIds.Contains(i.Id))
            .Select(i => new { i.Id, InvoiceNumber = i.Invoice.Number })
            .ToListAsync(ct);
        foreach (var fee in fees)
        {
            labels[fee.Id] = $"Factoring fee on invoice {Num(fee.InvoiceNumber)}";
        }

        return labels;
    }

    private static string Num(long number) => number.ToString(CultureInfo.InvariantCulture);
}
